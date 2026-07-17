namespace AquaDex.Core.DTOs;

public class BiteAlertDto
{
    public int Id { get; set; }
    public string PostedByDisplayName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int? SpeciesId { get; set; }
    public string? SpeciesCommonNameEn { get; set; }
    public string? Message { get; set; }
    public DateTime PostedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}