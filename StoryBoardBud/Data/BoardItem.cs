namespace StoryBoardBud.Data;

public class BoardItem
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public Board Board { get; set; } = null!;
    public Guid? PhotoId { get; set; }
    public Photo? Photo { get; set; }
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
