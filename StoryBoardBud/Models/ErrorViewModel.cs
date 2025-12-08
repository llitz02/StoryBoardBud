namespace StoryBoardBud.Models;

/// <summary>
/// View model for displaying error pages
/// </summary>
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
