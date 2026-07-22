using AquaDex.Core.Enums;

namespace AquaDex.Core.Entities;

public class ExpertConsultation
{
    public int Id { get; set; }

    public string RequesterUserId { get; set; } = string.Empty;
    public ApplicationUser RequesterUser { get; set; } = null!;

    public string ExpertUserId { get; set; } = string.Empty;
    public ApplicationUser ExpertUser { get; set; } = null!;

    public string Subject { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public ConsultationStatus Status { get; set; } = ConsultationStatus.Requested;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
    public string? ExpertResponse { get; set; }
}