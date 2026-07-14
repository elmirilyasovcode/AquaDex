namespace AquaDex.Core.Entities;

public class CatchLog
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int SpeciesId { get; set; }
    public Species Species { get; set; } = null!;

    public decimal? WeightKg { get; set; }
    public decimal? LengthCm { get; set; }
    public string? PhotoUrl { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool ShareExactLocation { get; set; } = false;

    public DateTime CaughtAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? Notes { get; set; }
}