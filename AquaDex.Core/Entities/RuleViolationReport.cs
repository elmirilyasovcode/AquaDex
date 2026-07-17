using AquaDex.Core.Enums;

namespace AquaDex.Core.Entities;

public class RuleViolationReport
{
    public int Id { get; set; }

    public string ReportedByUserId { get; set; } = string.Empty;
    public ApplicationUser ReportedByUser { get; set; } = null!;

    public ViolationType ViolationType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }

    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    public int? WaterbodyId { get; set; }
    public Waterbody? Waterbody { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
}