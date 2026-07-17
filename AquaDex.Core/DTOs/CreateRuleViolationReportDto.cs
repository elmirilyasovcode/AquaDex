using AquaDex.Core.Enums;

namespace AquaDex.Core.DTOs;

public class CreateRuleViolationReportDto
{
    public ViolationType ViolationType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int? WaterbodyId { get; set; }
}