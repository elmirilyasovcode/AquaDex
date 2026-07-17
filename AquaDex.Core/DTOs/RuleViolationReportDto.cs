using AquaDex.Core.Enums;

namespace AquaDex.Core.DTOs;

public class RuleViolationReportDto
{
    public int Id { get; set; }
    public string ReportedByUserId { get; set; } = string.Empty;
    public string ReportedByDisplayName { get; set; } = string.Empty;

    public ViolationType ViolationType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }

    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    public int? WaterbodyId { get; set; }
    public string? WaterbodyName { get; set; }

    public ReportStatus Status { get; set; }
    public DateTime ReportedAt { get; set; }
}