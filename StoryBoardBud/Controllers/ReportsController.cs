using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryBoardBud.Data;

namespace StoryBoardBud.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<ReportsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> CreateReport(Guid photoId, string reason, string? description)
    {
        var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == photoId);
        if (photo == null)
            return NotFound("Photo not found");

        var userId = _userManager.GetUserId(User);

        // Check if user already reported this photo
        var existingReport = await _context.Reports
            .FirstOrDefaultAsync(r => r.PhotoId == photoId && r.ReportedById == userId);
        
        if (existingReport != null)
            return BadRequest("You have already reported this photo");

        var report = new Report
        {
            Id = Guid.NewGuid(),
            PhotoId = photoId,
            ReportedById = userId!,
            Reason = reason,
            Description = description,
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Report created: {report.Id} for Photo: {photoId} by User: {userId}");

        return Ok(new { id = report.Id, message = "Report submitted successfully" });
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingReports(int page = 1, int pageSize = 10)
    {
        var skip = (page - 1) * pageSize;
        var reports = await _context.Reports
            .Where(r => r.Status == ReportStatus.Pending)
            .Include(r => r.Photo)
            .Include(r => r.ReportedBy)
            .OrderBy(r => r.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.Reports.Where(r => r.Status == ReportStatus.Pending).CountAsync();

        return Ok(new
        {
            data = reports,
            totalCount,
            pageSize,
            currentPage = page,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpPost("approve/{id}")]
    [Authorize(Roles = "Admin")]
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

        // Mark photo for deletion (or set IsDeleted flag if we add one)
        if (report.Photo != null)
        {
            report.Photo.IsPrivate = true; // Hide the photo
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Report {id} approved by Admin: {userId}");

        return Ok(new { message = "Report approved" });
    }

    [HttpPost("reject/{id}")]
    [Authorize(Roles = "Admin")]
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

        return Ok(new { message = "Report rejected" });
    }
}
