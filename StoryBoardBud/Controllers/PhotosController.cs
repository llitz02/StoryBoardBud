using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryBoardBud.Data;
using StoryBoardBud.Services;

namespace StoryBoardBud.Controllers;

/// <summary>
/// Manages photo uploads and board item operations
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PhotosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<PhotosController> _logger;

    /// <summary>
    /// Initializes the PhotosController with required services
    /// </summary>
    /// <param name="context">Database context for data access</param>
    /// <param name="userManager">User manager for authentication</param>
    /// <param name="fileStorageService">Service for file operations</param>
    /// <param name="logger">Logger for recording events</param>
    public PhotosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, 
        IFileStorageService fileStorageService, ILogger<PhotosController> logger)
    {
        _context = context;
        _userManager = userManager;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all photos (optionally filtered by privacy and user)
    /// </summary>
    /// <param name="includePrivate">Include private photos for the current user</param>
    /// <returns>List of photos</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll(bool includePrivate = false)
    {
        var userId = _userManager.GetUserId(User);
        
        var query = _context.Photos
            .Include(p => p.UploadedBy)
            .AsQueryable();

        if (!includePrivate)
        {
            query = query.Where(p => !p.IsPrivate || p.UploadedById == userId);
        }
        else if (userId != null)
        {
            query = query.Where(p => p.UploadedById == userId);
        }

        var photos = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id,
                p.FileName,
                p.FilePath,
                p.FileSizeBytes,
                p.IsPrivate,
                p.CreatedAt,
                UploadedBy = new
                {
                    p.UploadedBy.Id,
                    p.UploadedBy.UserName
                }
            })
            .ToListAsync();

        return Ok(photos);
    }

    /// <summary>
    /// Gets a specific photo by ID
    /// </summary>
    /// <param name="id">Photo ID</param>
    /// <returns>Photo details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var photo = await _context.Photos
            .Include(p => p.UploadedBy)
            .Include(p => p.BoardItems)
            .Include(p => p.Reports)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (photo == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (photo.IsPrivate && photo.UploadedById != userId)
            return Forbid();

        return Ok(new
        {
            photo.Id,
            photo.FileName,
            photo.FilePath,
            photo.FileSizeBytes,
            photo.IsPrivate,
            photo.CreatedAt,
            UploadedBy = new
            {
                photo.UploadedBy.Id,
                photo.UploadedBy.UserName
            },
            BoardItemCount = photo.BoardItems.Count,
            ReportCount = photo.Reports.Count
        });
    }

    /// <summary>
    /// Uploads a new photo to the server
    /// </summary>
    /// <param name="file">Image file to upload</param>
    /// <param name="isPrivate">Whether photo is private</param>
    /// <returns>JSON with photo ID and URL</returns>
    [HttpPost("upload")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Upload(IFormFile file, bool isPrivate = false)
    {
        // Server-side validation
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        // Validate file size (10MB max)
        if (file.Length > 10485760)
            return BadRequest("File size exceeds 10MB limit");

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return BadRequest("Invalid file type. Only images are allowed (jpg, jpeg, png, gif, webp)");

        try
        {
            var userId = _userManager.GetUserId(User);
            var filePath = await _fileStorageService.UploadPhotoAsync(file, userId!);

            var photo = new Photo
            {
                Id = Guid.NewGuid(),
                FileName = file.FileName,
                FilePath = filePath,
                FileSizeBytes = file.Length,
                UploadedById = userId!,
                IsPrivate = isPrivate,
                CreatedAt = DateTime.UtcNow
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return Ok(new { id = photo.Id, filePath = photo.FilePath, url = _fileStorageService.GetPhotoUrl(filePath) });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Upload failed: {ex.Message}");
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Gets all board items for a specific board
    /// </summary>
    /// <param name="boardId">Board ID</param>
    /// <returns>List of board items</returns>
    [HttpGet("board-items/{boardId}")]
    public async Task<IActionResult> GetBoardItems(Guid boardId)
    {
        var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == boardId);
        if (board == null)
            return NotFound("Board not found");

        var userId = _userManager.GetUserId(User);
        if (board.OwnerId != userId)
            return Forbid();

        var items = await _context.BoardItems
            .Where(bi => bi.BoardId == boardId)
            .Include(bi => bi.Photo)
                .ThenInclude(p => p!.UploadedBy)
            .OrderBy(bi => bi.ZIndex)
            .Select(bi => new
            {
                bi.Id,
                bi.BoardId,
                bi.PhotoId,
                bi.TextContent,
                bi.PositionX,
                bi.PositionY,
                bi.Width,
                bi.Height,
                bi.Rotation,
                bi.ZIndex,
                bi.CreatedAt,
                Photo = bi.Photo != null ? new
                {
                    bi.Photo.Id,
                    bi.Photo.FileName,
                    bi.Photo.FilePath,
                    UploadedBy = bi.Photo.UploadedBy.UserName
                } : null
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Gets a specific board item by ID
    /// </summary>
    /// <param name="id">Board item ID</param>
    /// <returns>Board item details</returns>
    [HttpGet("board-item/{id}")]
    public async Task<IActionResult> GetBoardItem(Guid id)
    {
        var item = await _context.BoardItems
            .Include(bi => bi.Board)
            .Include(bi => bi.Photo)
                .ThenInclude(p => p!.UploadedBy)
            .FirstOrDefaultAsync(bi => bi.Id == id);

        if (item == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (item.Board?.OwnerId != userId)
            return Forbid();

        return Ok(new
        {
            item.Id,
            item.BoardId,
            item.PhotoId,
            item.TextContent,
            item.PositionX,
            item.PositionY,
            item.Width,
            item.Height,
            item.Rotation,
            item.ZIndex,
            item.CreatedAt,
            item.UpdatedAt,
            Photo = item.Photo != null ? new
            {
                item.Photo.Id,
                item.Photo.FileName,
                item.Photo.FilePath,
                UploadedBy = item.Photo.UploadedBy?.UserName
            } : null
        });
    }

    /// <summary>
    /// Adds a photo to a board at specified position
    /// </summary>
    /// <param name="request">Request with board ID, photo ID, and position</param>
    /// <returns>JSON with new board item ID</returns>
    [HttpPost("add-to-board")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> AddToBoard([FromBody] AddToBoardRequest request)
    {
        // Server-side validation
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == request.BoardId);
        if (board == null)
            return NotFound("Board not found");

        var userId = _userManager.GetUserId(User);
        if (board.OwnerId != userId)
            return Forbid();

        var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == request.PhotoId);
        if (photo == null)
            return NotFound("Photo not found");

        var boardItem = new BoardItem
        {
            Id = Guid.NewGuid(),
            BoardId = request.BoardId,
            PhotoId = request.PhotoId,
            PositionX = request.PosX,
            PositionY = request.PosY,
            Width = request.Width,
            Height = request.Height,
            ZIndex = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.BoardItems.Add(boardItem);
        await _context.SaveChangesAsync();

        return Ok(new { id = boardItem.Id });
    }

    /// <summary>
    /// Updates position, size, and rotation of a board item
    /// </summary>
    /// <param name="request">Request with item ID and new properties</param>
    /// <returns>OK result on success</returns>
    [HttpPut("update-item")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdateItem([FromBody] UpdateItemRequest request)
    {
        // Server-side validation
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var item = await _context.BoardItems.FirstOrDefaultAsync(bi => bi.Id == request.ItemId);
        if (item == null)
            return NotFound();

        var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == item.BoardId);
        var userId = _userManager.GetUserId(User);
        if (board?.OwnerId != userId)
            return Forbid();

        item.PositionX = request.PosX;
        item.PositionY = request.PosY;
        item.Width = request.Width;
        item.Height = request.Height;
        item.Rotation = request.Rotation;
        item.ZIndex = request.ZIndex;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Adds a text element to a board
    /// </summary>
    /// <param name="request">Request with board ID, text, and position</param>
    /// <returns>JSON with new item ID</returns>
    [HttpPost("add-text")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> AddText([FromBody] AddTextRequest request)
    {
        // Server-side validation
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == request.BoardId);
        if (board == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (board.OwnerId != userId)
            return Forbid();

        var boardItem = new BoardItem
        {
            Id = Guid.NewGuid(),
            BoardId = request.BoardId,
            TextContent = request.Text,
            PositionX = request.PosX,
            PositionY = request.PosY,
            Width = 300,
            Height = 100,
            CreatedAt = DateTime.UtcNow
        };

        _context.BoardItems.Add(boardItem);
        await _context.SaveChangesAsync();

        return Ok(new { id = boardItem.Id });
    }

    /// <summary>
    /// Updates photo metadata (privacy setting)
    /// </summary>
    /// <param name="id">Photo ID</param>
    /// <param name="isPrivate">Privacy setting</param>
    /// <returns>Updated photo details</returns>
    [HttpPut("{id}")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdatePhoto(Guid id, [FromBody] bool isPrivate)
    {
        var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
        if (photo == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (photo.UploadedById != userId)
            return Forbid();

        photo.IsPrivate = isPrivate;
        await _context.SaveChangesAsync();

        return Ok(new { id = photo.Id, isPrivate = photo.IsPrivate, message = "Photo updated successfully" });
    }

    /// <summary>
    /// Deletes a photo and its file
    /// </summary>
    /// <param name="id">Photo ID to delete</param>
    /// <returns>OK result on success</returns>
    [HttpDelete("{id}")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
        if (photo == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (photo.UploadedById != userId)
            return Forbid();

        await _fileStorageService.DeletePhotoAsync(photo.FilePath);

        _context.Photos.Remove(photo);
        await _context.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Removes an item from a board
    /// </summary>
    /// <param name="id">Board item ID to delete</param>
    /// <returns>OK result on success</returns>
    [HttpDelete("item/{id}")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> DeleteItem(Guid id)
    {
        var item = await _context.BoardItems
            .Include(bi => bi.Board)
            .FirstOrDefaultAsync(bi => bi.Id == id);

        if (item == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (item.Board?.OwnerId != userId)
            return Forbid();

        _context.BoardItems.Remove(item);
        await _context.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Updates the text content of a board item via AJAX
    /// </summary>
    /// <param name="request">Request containing item ID and new text content</param>
    /// <returns>OK response on success</returns>
    [HttpPut("update-text")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdateText([FromBody] UpdateTextRequest request)
    {
        var item = await _context.BoardItems
            .Include(bi => bi.Board)
            .FirstOrDefaultAsync(bi => bi.Id == request.ItemId);

        if (item == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (item.Board?.OwnerId != userId)
            return Forbid();

        item.TextContent = request.TextContent;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok();
    }
}

// Request models
public class UpdateTextRequest
{
    public Guid ItemId { get; set; }
    public string TextContent { get; set; } = string.Empty;
}
