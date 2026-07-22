using AquaDex.Core.DTOs;
using AquaDex.Core.Enums;
using AquaDex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AquaDex.Infrastructure.Services;

public class BadgeService
{
    private readonly AquaDexDbContext _context;

    public BadgeService(AquaDexDbContext context)
    {
        _context = context;
    }

    public async Task<List<BadgeDto>> GetBadgesForUserAsync(string userId)
    {
        var catchCount = await _context.CatchLogs.CountAsync(c => c.UserId == userId);

        var confirmedViolationReports = await _context.RuleViolationReports
            .CountAsync(r => r.ReportedByUserId == userId && r.Status == ReportStatus.Confirmed);

        var totalSpecies = await _context.Species.CountAsync();
        var discoveredSpecies = await _context.CatchLogs
            .Where(c => c.UserId == userId)
            .Select(c => c.SpeciesId)
            .Distinct()
            .CountAsync();

        var codexComplete = totalSpecies > 0 && discoveredSpecies == totalSpecies;

        return new List<BadgeDto>
        {
            new() { Name = "First Catch", Description = "Log your first catch.", Earned = catchCount >= 1 },
            new() { Name = "Sturgeon Guardian", Description = "Report a confirmed poaching or protected species violation.", Earned = confirmedViolationReports >= 1 },
            new() { Name = "Encyclopedist", Description = "Complete 100% of the Species Codex.", Earned = codexComplete },
            new() { Name = "Dedicated Angler", Description = "Log 10 or more catches.", Earned = catchCount >= 10 }
        };
    }
}