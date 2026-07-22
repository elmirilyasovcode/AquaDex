using AquaDex.Core.Entities;
using AquaDex.Core.Enums;
using AquaDex.Infrastructure.Data;

namespace AquaDex.Infrastructure.Services;

public class PointsService
{
    private readonly AquaDexDbContext _context;

    private static readonly Dictionary<PointsReason, int> PointValues = new()
    {
        { PointsReason.CatchLogged, 5 },
        { PointsReason.DatabankContributionApproved, 20 },
        { PointsReason.ForumThreadPosted, 2 },
        { PointsReason.ForumReplyPosted, 1 },
        { PointsReason.ForumBestAnswer, 10 },
        { PointsReason.ViolationReportConfirmed, 15 }
    };

    public PointsService(AquaDexDbContext context)
    {
        _context = context;
    }

    public async Task AwardPointsAsync(string userId, PointsReason reason, int? referenceId = null)
    {
        var points = PointValues.GetValueOrDefault(reason, 0);
        if (points == 0)
            return;

        _context.PointsTransactions.Add(new PointsTransaction
        {
            UserId = userId,
            Points = points,
            Reason = reason,
            ReferenceId = referenceId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }
}