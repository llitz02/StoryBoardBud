using Microsoft.AspNetCore.Identity;

namespace StoryBoardBud.Data;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? BioDescription { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Board> Boards { get; set; } = new List<Board>();
    public ICollection<Photo> UploadedPhotos { get; set; } = new List<Photo>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}
