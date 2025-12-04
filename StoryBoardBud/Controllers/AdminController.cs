using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryBoardBud.Data;
using StoryBoardBud.Services;

namespace StoryBoardBud.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager, IFileStorageService fileStorageService, ILogger<AdminController> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Users(int page = 1, int pageSize = 10)
    {
        var skip = (page - 1) * pageSize;
        var users = await _userManager.Users
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _userManager.Users.CountAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.TotalCount = totalCount;

        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> UserDetail(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var boards = await _context.Boards.Where(b => b.OwnerId == id).CountAsync();
        var photos = await _context.Photos.Where(p => p.UploadedById == id).CountAsync();

        ViewBag.Roles = roles;
        ViewBag.BoardsCount = boards;
        ViewBag.PhotosCount = photos;

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LockUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
        await _userManager.UpdateAsync(user);

        _logger.LogInformation($"User {id} locked by Admin");
        return RedirectToAction(nameof(UserDetail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnlockUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        user.LockoutEnd = null;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation($"User {id} unlocked by Admin");
        return RedirectToAction(nameof(UserDetail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        // Delete user's boards (cascades to items)
        var boards = await _context.Boards.Where(b => b.OwnerId == id).ToListAsync();
        foreach (var board in boards)
        {
            _context.Boards.Remove(board);
        }

        // Delete user's photos
        var photos = await _context.Photos.Where(p => p.UploadedById == id).ToListAsync();
        foreach (var photo in photos)
        {
            await _fileStorageService.DeletePhotoAsync(photo.FilePath);
            _context.Photos.Remove(photo);
        }

        await _context.SaveChangesAsync();
        await _userManager.DeleteAsync(user);

        _logger.LogInformation($"User {id} deleted by Admin");
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public async Task<IActionResult> Reports(ReportStatus? status = null, int page = 1, int pageSize = 10)
    {
        var skip = (page - 1) * pageSize;
        
        var query = _context.Reports
            .Include(r => r.Photo)
            .Include(r => r.ReportedBy)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status);

        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.Reports
            .Where(r => !status.HasValue || r.Status == status)
            .CountAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.TotalCount = totalCount;
        ViewBag.SelectedStatus = status;

        return View(reports);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveReport(Guid id, string? adminNotes)
    {
        var report = await _context.Reports
            .Include(r => r.Photo)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        report.Status = ReportStatus.Approved;
        report.ReviewedAt = DateTime.UtcNow;
        report.ReviewedById = userId;
        report.AdminNotes = adminNotes;

        // Hide the photo
        if (report.Photo != null)
        {
            report.Photo.IsPrivate = true;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Report {id} approved by Admin: {userId}");
        return RedirectToAction(nameof(Reports), new { status = ReportStatus.Pending });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectReport(Guid id, string? adminNotes)
    {
        var report = await _context.Reports.FirstOrDefaultAsync(r => r.Id == id);
        if (report == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        report.Status = ReportStatus.Rejected;
        report.ReviewedAt = DateTime.UtcNow;
        report.ReviewedById = userId;
        report.AdminNotes = adminNotes;

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Report {id} rejected by Admin: {userId}");
        return RedirectToAction(nameof(Reports), new { status = ReportStatus.Pending });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhoto(Guid id)
    {
        var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
        if (photo == null)
            return NotFound();

        await _fileStorageService.DeletePhotoAsync(photo.FilePath);

        _context.Photos.Remove(photo);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Photo {id} deleted by Admin");
        return RedirectToAction(nameof(Reports), new { status = ReportStatus.Pending });
    }
}
