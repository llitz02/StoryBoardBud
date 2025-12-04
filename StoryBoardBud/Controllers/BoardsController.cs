using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryBoardBud.Data;

namespace StoryBoardBud.Controllers;

[Authorize]
public class BoardsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<BoardsController> _logger;

    public BoardsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<BoardsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var boards = await _context.Boards
            .Include(b => b.Owner)
            .ToListAsync();
        return View(boards);
    }

    public async Task<IActionResult> MyBoards()
    {
        var userId = _userManager.GetUserId(User);
        var boards = await _context.Boards
            .Where(b => b.OwnerId == userId)
            .Include(b => b.Items)
            .ToListAsync();
        return View(boards);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string title, string? description)
    {
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

        return RedirectToAction(nameof(Edit), new { id = board.Id });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var board = await _context.Boards
            .Include(b => b.Items)
            .ThenInclude(bi => bi.Photo)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (board == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (board.OwnerId != userId)
            return Forbid();

        return View(board);
    }

    [HttpPost]
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
        return RedirectToAction(nameof(Edit), new { id });
    }

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
}
