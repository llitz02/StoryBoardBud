namespace StoryBoardBud.Controllers;

public class AddToBoardRequest
{
    public Guid BoardId { get; set; }
    public Guid PhotoId { get; set; }
    public double PosX { get; set; }
    public double PosY { get; set; }
    public double Width { get; set; } = 200;
    public double Height { get; set; } = 200;
}

public class AddTextRequest
{
    public Guid BoardId { get; set; }
    public string Text { get; set; } = string.Empty;
    public double PosX { get; set; }
    public double PosY { get; set; }
}

public class UpdateItemRequest
{
    public Guid ItemId { get; set; }
    public double PosX { get; set; }
    public double PosY { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Rotation { get; set; }
    public int ZIndex { get; set; }
}

public class CreateReportRequest
{
    public Guid PhotoId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
}
