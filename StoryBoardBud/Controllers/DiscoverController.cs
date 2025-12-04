using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryBoardBud.Data;

namespace StoryBoardBud.Controllers;

public class DiscoverController : Controller
{
    private readonly ApplicationDbContext _context;

    public DiscoverController(ApplicationDbContext context)
    {
        _context = context;
    }

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
