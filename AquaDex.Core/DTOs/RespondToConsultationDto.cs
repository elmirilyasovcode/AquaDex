using AquaDex.Core.Enums;

namespace AquaDex.Core.DTOs;

public class RespondToConsultationDto
{
    public ConsultationStatus Status { get; set; }
    public string? ExpertResponse { get; set; }
}