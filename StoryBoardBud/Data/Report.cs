namespace StoryBoardBud.Data;

public class Report
{
    public Guid Id { get; set; }
    public Guid PhotoId { get; set; }
    public Photo Photo { get; set; } = null!;
    public string ReportedById { get; set; } = string.Empty;
    public ApplicationUser ReportedBy { get; set; } = null!;
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedById { get; set; }
    public ApplicationUser? ReviewedBy { get; set; }
    public string? AdminNotes { get; set; }
}

public enum ReportStatus
{
    Pending,
    Reviewed,
    Approved,
    Rejected
}
