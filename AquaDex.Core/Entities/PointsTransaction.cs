using AquaDex.Core.Enums;

namespace AquaDex.Core.Entities;

public class PointsTransaction
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int Points { get; set; }
    public PointsReason Reason { get; set; }
    public int? ReferenceId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}