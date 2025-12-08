using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryBoardBud.Data;

namespace StoryBoardBud.Controllers;

/// <summary>
/// Manages user's favorite photos
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FavoritesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Initializes the FavoritesController with database and user management
    /// </summary>
    /// <param name="context">Database context for data access</param>
    /// <param name="userManager">User manager for authentication</param>
    public FavoritesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// Adds a photo to user's favorites
    /// </summary>
    /// <param name="photoId">Photo ID to favorite</param>
    /// <returns>Success message or error</returns>
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

    /// <summary>
    /// Removes a photo from user's favorites
    /// </summary>
    /// <param name="photoId">Photo ID to unfavorite</param>
    /// <returns>Success message or error</returns>
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

    /// <summary>
    /// Gets all favorite photos for current user
    /// </summary>
    /// <returns>JSON with favorited photos</returns>
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

    /// <summary>
    /// Checks if a photo is favorited by current user
    /// </summary>
    /// <param name="photoId">Photo ID to check</param>
    /// <returns>JSON with favorite status</returns>
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
