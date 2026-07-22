namespace AquaDex.Core.Entities;

public class GuideListing
{
    public int Id { get; set; }

    public string GuideUserId { get; set; } = string.Empty;
    public ApplicationUser GuideUser { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}