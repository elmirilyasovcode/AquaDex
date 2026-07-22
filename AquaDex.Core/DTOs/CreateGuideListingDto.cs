namespace AquaDex.Core.DTOs;

public class CreateGuideListingDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
}