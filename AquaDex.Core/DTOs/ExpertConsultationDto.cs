namespace AquaDex.Core.DTOs;

public class ExpertConsultationDto
{
    public int Id { get; set; }
    public string RequesterDisplayName { get; set; } = string.Empty;
    public string ExpertUserId { get; set; } = string.Empty;
    public string ExpertDisplayName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? ExpertResponse { get; set; }
}