using System.ComponentModel.DataAnnotations;

namespace StoryBoardBud.Data;

/// <summary>
/// Represents a storyboard containing photos and text items
/// </summary>
public class Board
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public string OwnerId { get; set; } = string.Empty;
    public ApplicationUser Owner { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<BoardItem> Items { get; set; } = new List<BoardItem>();
}
