using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryBoardBud.Data;

namespace StoryBoardBud.Controllers;

/// <summary>
/// Manages storyboard creation, editing, and viewing
/// </summary>
[Authorize]
public class BoardsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<BoardsController> _logger;

    /// <summary>
    /// Initializes the BoardsController with database and user management
    /// </summary>
    /// <param name="context">Database context for data access</param>
    /// <param name="userManager">User manager for authentication</param>
    /// <param name="logger">Logger for recording events</param>
    public BoardsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<BoardsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Displays all boards with owner information
    /// </summary>
    /// <returns>View with list of all boards</returns>
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var boards = await _context.Boards
            .Include(b => b.Owner)
            .ToListAsync();
        return View(boards);
    }

    /// <summary>
    /// Displays boards owned by the current user
    /// </summary>
    /// <returns>View with user's boards</returns>
    public async Task<IActionResult> MyBoards()
    {
        var userId = _userManager.GetUserId(User);
        var boards = await _context.Boards
            .Where(b => b.OwnerId == userId)
            .Include(b => b.Items)
            .ToListAsync();
        return View(boards);
    }

    /// <summary>
    /// Displays the board creation form
    /// </summary>
    /// <returns>Board creation view</returns>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Creates a new board - supports both traditional and AJAX form submission
    /// </summary>
    /// <param name="title">Board title</param>
    /// <param name="description">Optional board description</param>
    /// <returns>Redirect to edit page or JSON response for AJAX</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string title, string? description)
    {
        // Server-side validation
        if (string.IsNullOrWhiteSpace(title))
        {
            ModelState.AddModelError(nameof(title), "Title is required.");
        }
        else if (title.Length > 100)
        {
            ModelState.AddModelError(nameof(title), "Title cannot exceed 100 characters.");
        }

        if (description?.Length > 500)
        {
            ModelState.AddModelError(nameof(description), "Description cannot exceed 500 characters.");
        }

        if (!ModelState.IsValid)
        {
            return View();
        }

        var userId = _userManager.GetUserId(User);
        var board = new Board
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            OwnerId = userId!,
            CreatedAt = DateTime.UtcNow
        };

        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        // Check if this is an AJAX request
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { 
                id = board.Id, 
                title = board.Title,
                redirectUrl = Url.Action(nameof(Edit), new { id = board.Id })
            });
        }

        return RedirectToAction(nameof(Edit), new { id = board.Id });
    }

    /// <summary>
    /// Displays the board editor for a specific board
    /// </summary>
    /// <param name="id">Board ID to edit</param>
    /// <returns>Board editor view or error if not found/unauthorized</returns>
    public async Task<IActionResult> Edit(Guid id)
    {
        var board = await _context.Boards
            .Include(b => b.Items)
            .ThenInclude(bi => bi.Photo)
                .ThenInclude(p => p!.UploadedBy)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (board == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (board.OwnerId != userId)
            return Forbid();

        return View(board);
    }

    /// <summary>
    /// Updates an existing board - supports both traditional and AJAX form submission
    /// </summary>
    /// <param name="id">Board ID to update</param>
    /// <param name="title">New board title</param>
    /// <param name="description">New board description</param>
    /// <returns>Redirect to edit page or JSON response for AJAX</returns>
    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, string title, string? description)
    {
        var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == id);
        if (board == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (board.OwnerId != userId)
            return Forbid();

        board.Title = title;
        board.Description = description;
        board.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Check if this is an AJAX request
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { 
                id = board.Id, 
                title = board.Title,
                description = board.Description,
                success = true
            });
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    /// <summary>
    /// REST API endpoint to update a board
    /// </summary>
    /// <param name="id">Board ID</param>
    /// <param name="title">New title</param>
    /// <param name="description">New description</param>
    /// <returns>Updated board data</returns>
    [HttpPut("api/boards/{id}")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdateBoard(Guid id, [FromBody] UpdateBoardRequest request)
    {
        // Server-side validation
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == id);
        if (board == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (board.OwnerId != userId)
            return Forbid();

        board.Title = request.Title;
        board.Description = request.Description;
        board.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = board.Id,
            title = board.Title,
            description = board.Description,
            updatedAt = board.UpdatedAt
        });
    }

    /// <summary>
    /// Deletes a board and all its items
    /// </summary>
    /// <param name="id">Board ID to delete</param>
    /// <returns>Redirect to MyBoards page</returns>
    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == id);
        if (board == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (board.OwnerId != userId)
            return Forbid();

        _context.Boards.Remove(board);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(MyBoards));
    }

    /// <summary>
    /// Gets board details with all items
    /// </summary>
    /// <param name="id">Board ID to retrieve</param>
    /// <returns>JSON with board data</returns>
    [HttpGet]
    public async Task<IActionResult> GetBoard(Guid id)
    {
        var board = await _context.Boards
            .Include(b => b.Items)
            .ThenInclude(bi => bi.Photo)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (board == null)
            return NotFound();

        return Json(board);
    }

    /// <summary>
    /// Gets all boards owned by current user
    /// </summary>
    /// <returns>JSON with list of user's boards</returns>
    [HttpGet("api/boards/my-boards")]
    public async Task<IActionResult> GetMyBoards()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var boards = await _context.Boards
            .Where(b => b.OwnerId == userId)
            .OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt)
            .Select(b => new
            {
                b.Id,
                b.Title,
                b.Description
            })
            .ToListAsync();

        return Json(boards);
    }

    /// <summary>
    /// Displays user's favorite photos
    /// </summary>
    /// <returns>View with favorited photos</returns>
    public async Task<IActionResult> MyFavorites()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var favorites = await _context.FavoritePhotos
            .Where(f => f.UserId == userId)
            .Include(f => f.Photo)
                .ThenInclude(p => p.UploadedBy)
            .OrderByDescending(f => f.FavoritedAt)
            .ToListAsync();

        return View(favorites);
    }
}
