namespace AquaDex.Core.DTOs;

public class CreateConsultationDto
{
    public string ExpertUserId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
}