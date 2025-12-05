using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryBoardBud.Data;

namespace StoryBoardBud.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FavoritesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public FavoritesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost("{photoId}")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> AddFavorite(Guid photoId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var photo = await _context.Photos.FindAsync(photoId);
        if (photo == null)
            return NotFound("Photo not found");

        // Check if already favorited
        var existing = await _context.FavoritePhotos
            .FirstOrDefaultAsync(f => f.UserId == userId && f.PhotoId == photoId);
        
        if (existing != null)
            return BadRequest("Already favorited");

        var favorite = new FavoritePhoto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PhotoId = photoId
        };

        _context.FavoritePhotos.Add(favorite);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Added to favorites" });
    }

    [HttpDelete("{photoId}")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> RemoveFavorite(Guid photoId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var favorite = await _context.FavoritePhotos
            .FirstOrDefaultAsync(f => f.UserId == userId && f.PhotoId == photoId);
        
        if (favorite == null)
            return NotFound("Favorite not found");

        _context.FavoritePhotos.Remove(favorite);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Removed from favorites" });
    }

    [HttpGet]
    public async Task<IActionResult> GetMyFavorites()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var favorites = await _context.FavoritePhotos
            .Where(f => f.UserId == userId)
            .Include(f => f.Photo)
                .ThenInclude(p => p.UploadedBy)
            .OrderByDescending(f => f.FavoritedAt)
            .Select(f => new
            {
                f.Id,
                f.PhotoId,
                f.FavoritedAt,
                Photo = new
                {
                    f.Photo.Id,
                    f.Photo.FileName,
                    f.Photo.FilePath,
                    f.Photo.CreatedAt,
                    UploadedBy = f.Photo.UploadedBy.UserName
                }
            })
            .ToListAsync();

        return Ok(favorites);
    }

    [HttpGet("check/{photoId}")]
    public async Task<IActionResult> CheckFavorite(Guid photoId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var isFavorited = await _context.FavoritePhotos
            .AnyAsync(f => f.UserId == userId && f.PhotoId == photoId);

        return Ok(new { isFavorited });
    }
}
