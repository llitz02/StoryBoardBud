using System.ComponentModel.DataAnnotations;

namespace StoryBoardBud.Data;

/// <summary>
/// Represents an uploaded photo file
/// </summary>
public class Photo
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [Range(1, 10485760)]
    public long FileSizeBytes { get; set; }
    
    [Required]
    public string UploadedById { get; set; } = string.Empty;
    public ApplicationUser UploadedBy { get; set; } = null!;
    public bool IsPrivate { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<BoardItem> BoardItems { get; set; } = new List<BoardItem>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
    public ICollection<FavoritePhoto> FavoritedByUsers { get; set; } = new List<FavoritePhoto>();
}
