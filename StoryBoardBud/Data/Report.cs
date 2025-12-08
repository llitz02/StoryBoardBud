using System.ComponentModel.DataAnnotations;

namespace StoryBoardBud.Data;

/// <summary>
/// Represents a user report about inappropriate content
/// </summary>
public class Report
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid PhotoId { get; set; }
    public Photo Photo { get; set; } = null!;
    
    [Required]
    public string ReportedById { get; set; } = string.Empty;
    public ApplicationUser ReportedBy { get; set; } = null!;
    
    [Required]
    [StringLength(100)]
    public string Reason { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedById { get; set; }
    public ApplicationUser? ReviewedBy { get; set; }
    
    [StringLength(1000)]
    public string? AdminNotes { get; set; }
}

public enum ReportStatus
{
    Pending,
    Reviewed,
    Approved,
    Rejected
}
