namespace AquaDex.Core.DTOs;

public class CatchLogDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty;

    public int SpeciesId { get; set; }
    public string SpeciesCommonNameEn { get; set; } = string.Empty;

    public decimal? WeightKg { get; set; }
    public decimal? LengthCm { get; set; }
    public string? PhotoUrl { get; set; }

    // Only populated if ShareExactLocation is true — enforced in the controller, not just the DTO shape
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool ShareExactLocation { get; set; }

    public DateTime CaughtAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
    public bool IsProtectedSpeciesCatch { get; set; }
}