namespace AquaDex.Core.DTOs;

public class GuideListingDto
{
    public int Id { get; set; }
    public string GuideUserId { get; set; } = string.Empty;
    public string GuideDisplayName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public DateTime CreatedAt { get; set; }
}