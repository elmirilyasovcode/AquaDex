namespace AquaDex.Core.DTOs;

public class CreateCatchLogDto
{
    public int SpeciesId { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? LengthCm { get; set; }
    public string? PhotoUrl { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool ShareExactLocation { get; set; } = false;
    public DateTime CaughtAt { get; set; }
    public string? Notes { get; set; }
}