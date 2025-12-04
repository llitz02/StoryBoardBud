using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryBoardBud.Data;
using StoryBoardBud.Services;

namespace StoryBoardBud.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PhotosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<PhotosController> _logger;

    public PhotosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, 
        IFileStorageService fileStorageService, ILogger<PhotosController> logger)
    {
        _context = context;
        _userManager = userManager;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    [HttpPost("upload")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Upload(IFormFile file, bool isPrivate = false)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

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

    [HttpPost("add-to-board")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> AddToBoard(Guid boardId, Guid photoId, double posX, double posY, double width = 200, double height = 200)
    {
        var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == boardId);
        if (board == null)
            return NotFound("Board not found");

        var userId = _userManager.GetUserId(User);
        if (board.OwnerId != userId)
            return Forbid();

        var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == photoId);
        if (photo == null)
            return NotFound("Photo not found");

        var boardItem = new BoardItem
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            PhotoId = photoId,
            PositionX = posX,
            PositionY = posY,
            Width = width,
            Height = height,
            ZIndex = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.BoardItems.Add(boardItem);
        await _context.SaveChangesAsync();

        return Ok(new { id = boardItem.Id });
    }

    [HttpPost("update-item")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdateItem(Guid itemId, double posX, double posY, double width, double height, double rotation, int zIndex)
    {
        var item = await _context.BoardItems.FirstOrDefaultAsync(bi => bi.Id == itemId);
        if (item == null)
            return NotFound();

        var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == item.BoardId);
        var userId = _userManager.GetUserId(User);
        if (board?.OwnerId != userId)
            return Forbid();

        item.PositionX = posX;
        item.PositionY = posY;
        item.Width = width;
        item.Height = height;
        item.Rotation = rotation;
        item.ZIndex = zIndex;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("add-text")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> AddText(Guid boardId, string text, double posX, double posY)
    {
        var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == boardId);
        if (board == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (board.OwnerId != userId)
            return Forbid();

        var boardItem = new BoardItem
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            TextContent = text,
            PositionX = posX,
            PositionY = posY,
            Width = 300,
            Height = 100,
            CreatedAt = DateTime.UtcNow
        };

        _context.BoardItems.Add(boardItem);
        await _context.SaveChangesAsync();

        return Ok(new { id = boardItem.Id });
    }

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
}
