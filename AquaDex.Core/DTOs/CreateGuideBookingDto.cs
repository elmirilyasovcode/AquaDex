namespace AquaDex.Core.DTOs;

public class CreateGuideBookingDto
{
    public int ListingId { get; set; }
    public DateTime RequestedDate { get; set; }
    public string? Notes { get; set; }
}