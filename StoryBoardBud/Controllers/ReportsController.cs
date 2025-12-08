using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryBoardBud.Data;

namespace StoryBoardBud.Controllers;

/// <summary>
/// Manages photo reports and moderation
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ReportsController> _logger;

    /// <summary>
    /// Initializes the ReportsController with required services
    /// </summary>
    /// <param name="context">Database context for data access</param>
    /// <param name="userManager">User manager for authentication</param>
    /// <param name="logger">Logger for recording events</param>
    public ReportsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<ReportsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Gets all reports with optional status filter
    /// </summary>
    /// <param name="status">Filter by report status</param>
    /// <returns>List of reports</returns>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllReports(ReportStatus? status = null)
    {
        var query = _context.Reports
            .Include(r => r.Photo)
            .Include(r => r.ReportedBy)
            .Include(r => r.ReviewedBy)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.PhotoId,
                r.Reason,
                r.Description,
                r.Status,
                r.CreatedAt,
                r.ReviewedAt,
                r.AdminNotes,
                ReportedBy = new
                {
                    r.ReportedBy.Id,
                    r.ReportedBy.UserName
                },
                ReviewedBy = r.ReviewedBy != null ? new
                {
                    r.ReviewedBy.Id,
                    r.ReviewedBy.UserName
                } : null,
                Photo = new
                {
                    r.Photo.Id,
                    r.Photo.FileName,
                    r.Photo.FilePath
                }
            })
            .ToListAsync();

        return Ok(reports);
    }

    /// <summary>
    /// Gets a specific report by ID
    /// </summary>
    /// <param name="id">Report ID</param>
    /// <returns>Report details</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var report = await _context.Reports
            .Include(r => r.Photo)
            .Include(r => r.ReportedBy)
            .Include(r => r.ReviewedBy)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null)
            return NotFound();

        return Ok(new
        {
            report.Id,
            report.PhotoId,
            report.Reason,
            report.Description,
            report.Status,
            report.CreatedAt,
            report.ReviewedAt,
            report.AdminNotes,
            ReportedBy = new
            {
                report.ReportedBy.Id,
                report.ReportedBy.UserName,
                report.ReportedBy.Email
            },
            ReviewedBy = report.ReviewedBy != null ? new
            {
                report.ReviewedBy.Id,
                report.ReviewedBy.UserName
            } : null,
            Photo = new
            {
                report.Photo.Id,
                report.Photo.FileName,
                report.Photo.FilePath,
                report.Photo.IsPrivate
            }
        });
    }

    /// <summary>
    /// Creates a new photo report
    /// </summary>
    /// <param name="request">Report details including photo ID and reason</param>
    /// <returns>JSON with report ID or error</returns>
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest request)
    {
        // Server-side validation
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == request.PhotoId);
        if (photo == null)
            return NotFound("Photo not found");

        var userId = _userManager.GetUserId(User);

        // Check if user already reported this photo
        var existingReport = await _context.Reports
            .FirstOrDefaultAsync(r => r.PhotoId == request.PhotoId && r.ReportedById == userId);
        
        if (existingReport != null)
            return BadRequest("You have already reported this photo");

        var report = new Report
        {
            Id = Guid.NewGuid(),
            PhotoId = request.PhotoId,
            ReportedById = userId!,
            Reason = request.Reason,
            Description = request.Description,
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Report created: {report.Id} for Photo: {request.PhotoId} by User: {userId}");

        return Ok(new { id = report.Id, message = "Report submitted successfully" });
    }

    /// <summary>
    /// Gets paginated pending reports for admin review
    /// </summary>
    /// <param name="page">Page number to retrieve</param>
    /// <param name="pageSize">Number of reports per page</param>
    /// <returns>JSON with pending reports</returns>
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

    /// <summary>
    /// Approves a report and hides the reported photo
    /// </summary>
    /// <param name="id">Report ID to approve</param>
    /// <param name="adminNotes">Optional admin notes</param>
    /// <returns>Success message</returns>
    [HttpPut("approve/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveReport(Guid id, [FromBody] string? adminNotes)
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

    /// <summary>
    /// Rejects a report without taking action
    /// </summary>
    /// <param name="id">Report ID to reject</param>
    /// <param name="adminNotes">Optional admin notes</param>
    /// <returns>Success message</returns>
    [HttpPut("reject/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectReport(Guid id, [FromBody] string? adminNotes)
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
