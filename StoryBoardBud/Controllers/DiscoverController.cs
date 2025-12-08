using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryBoardBud.Data;

namespace StoryBoardBud.Controllers;

/// <summary>
/// Displays public photos for browsing and discovery
/// </summary>
public class DiscoverController : Controller
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes the DiscoverController with database access
    /// </summary>
    /// <param name="context">Database context for data access</param>
    public DiscoverController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Displays paginated public photos
    /// </summary>
    /// <param name="page">Page number to display</param>
    /// <param name="pageSize">Number of photos per page</param>
    /// <returns>View with public photos</returns>
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 12)
    {
        var skip = (page - 1) * pageSize;
        var photos = await _context.Photos
            .Where(p => !p.IsPrivate)
            .Include(p => p.UploadedBy)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.Photos.Where(p => !p.IsPrivate).CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCount = totalCount;

        return View(photos);
    }

    /// <summary>
    /// Gets paginated public photos as JSON
    /// </summary>
    /// <param name="page">Page number to retrieve</param>
    /// <param name="pageSize">Number of photos per page</param>
    /// <returns>JSON with photo data and pagination info</returns>
    [HttpGet("api/photos")]
    public async Task<IActionResult> GetPhotos(int page = 1, int pageSize = 12)
    {
        var skip = (page - 1) * pageSize;
        var photos = await _context.Photos
            .Where(p => !p.IsPrivate)
            .Select(p => new 
            {
                p.Id,
                p.FileName,
                p.FilePath,
                p.CreatedAt,
                UploadedBy = p.UploadedBy.UserName
            })
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.Photos.Where(p => !p.IsPrivate).CountAsync();

        return Ok(new
        {
            data = photos,
            totalCount,
            pageSize,
            currentPage = page,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }
}
