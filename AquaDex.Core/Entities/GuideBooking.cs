using AquaDex.Core.Enums;

namespace AquaDex.Core.Entities;

public class GuideBooking
{
    public int Id { get; set; }

    public int ListingId { get; set; }
    public GuideListing Listing { get; set; } = null!;

    public string RequesterUserId { get; set; } = string.Empty;
    public ApplicationUser RequesterUser { get; set; } = null!;

    public DateTime RequestedDate { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Requested;
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
}