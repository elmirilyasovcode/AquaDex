namespace AquaDex.Core.DTOs;

public class GuideBookingDto
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public string RequesterDisplayName { get; set; } = string.Empty;
    public DateTime RequestedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}