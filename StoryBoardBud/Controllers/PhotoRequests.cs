using System.ComponentModel.DataAnnotations;

namespace StoryBoardBud.Controllers;

/// <summary>
/// Request model for updating board details
/// </summary>
public class UpdateBoardRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
}

/// <summary>
/// Request model for adding a photo to a board
/// </summary>
public class AddToBoardRequest
{
    [Required]
    public Guid BoardId { get; set; }
    
    [Required]
    public Guid PhotoId { get; set; }
    
    [Range(0, double.MaxValue)]
    public double PosX { get; set; }
    
    [Range(0, double.MaxValue)]
    public double PosY { get; set; }
    
    [Range(10, 2000)]
    public double Width { get; set; } = 200;
    
    [Range(10, 2000)]
    public double Height { get; set; } = 200;
}

/// <summary>
/// Request model for adding text to a board
/// </summary>
public class AddTextRequest
{
    [Required]
    public Guid BoardId { get; set; }
    
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Text { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue)]
    public double PosX { get; set; }
    
    [Range(0, double.MaxValue)]
    public double PosY { get; set; }
}

/// <summary>
/// Request model for updating board item position and size
/// </summary>
public class UpdateItemRequest
{
    [Required]
    public Guid ItemId { get; set; }
    
    [Range(0, double.MaxValue)]
    public double PosX { get; set; }
    
    [Range(0, double.MaxValue)]
    public double PosY { get; set; }
    
    [Range(10, 2000)]
    public double Width { get; set; }
    
    [Range(10, 2000)]
    public double Height { get; set; }
    
    [Range(-360, 360)]
    public double Rotation { get; set; }
    
    [Range(0, 1000)]
    public int ZIndex { get; set; }
}

/// <summary>
/// Request model for reporting inappropriate content
/// </summary>
public class CreateReportRequest
{
    [Required]
    public Guid PhotoId { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Reason { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
}
