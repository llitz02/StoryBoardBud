using System.ComponentModel.DataAnnotations;

namespace StoryBoardBud.Data;

/// <summary>
/// Represents a photo or text element on a board
/// </summary>
public class BoardItem
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid BoardId { get; set; }
    public Board Board { get; set; } = null!;
    
    public Guid? PhotoId { get; set; }
    public Photo? Photo { get; set; }
    
    [StringLength(1000)]
    public string? TextContent { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double Width { get; set; } = 200;
    public double Height { get; set; } = 200;
    public double Rotation { get; set; } = 0;
    public int ZIndex { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
