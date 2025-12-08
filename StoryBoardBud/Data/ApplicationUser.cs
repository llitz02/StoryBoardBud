using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace StoryBoardBud.Data;

/// <summary>
/// Extended user model with custom properties for StoryBoardBud application
/// </summary>
public class ApplicationUser : IdentityUser
{
    [StringLength(100)]
    public string? FullName { get; set; }
    
    [StringLength(500)]
    public string? BioDescription { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Board> Boards { get; set; } = new List<Board>();
    public ICollection<Photo> UploadedPhotos { get; set; } = new List<Photo>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
    public ICollection<FavoritePhoto> FavoritePhotos { get; set; } = new List<FavoritePhoto>();
}
