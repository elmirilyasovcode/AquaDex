namespace AquaDex.Core.Entities;

public class BiteAlert
{
    public int Id { get; set; }

    public string PostedByUserId { get; set; } = string.Empty;
    public ApplicationUser PostedByUser { get; set; } = null!;

    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    public int? SpeciesId { get; set; }
    public Species? Species { get; set; }

    public string? Message { get; set; }

    public DateTime PostedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
}