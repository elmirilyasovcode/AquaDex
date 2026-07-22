using AquaDex.Core.DTOs;
using AquaDex.Core.Entities;
using AquaDex.Infrastructure.Data;
using AquaDex.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaDex.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PointsController : ControllerBase
{
    private readonly AquaDexDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly BadgeService _badgeService;

    public PointsController(AquaDexDbContext context, UserManager<ApplicationUser> userManager, BadgeService badgeService)
    {
        _context = context;
        _userManager = userManager;
        _badgeService = badgeService;
    }

    // GET: api/points/mine
    [HttpGet("mine")]
    [Authorize]
    public async Task<ActionResult<PointsSummaryDto>> GetMyPoints()
    {
        var userId = _userManager.GetUserId(User);

        var transactions = await _context.PointsTransactions
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var total = transactions.Sum(t => t.Points);

        return Ok(new PointsSummaryDto
        {
            TotalPoints = total,
            RecentTransactions = transactions.Take(10).Select(t => new PointsTransactionDto
            {
                Points = t.Points,
                Reason = t.Reason.ToString(),
                CreatedAt = t.CreatedAt
            }).ToList()
        });
    }

    // GET: api/points/badges
    [HttpGet("badges")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<BadgeDto>>> GetMyBadges()
    {
        var userId = _userManager.GetUserId(User);
        var badges = await _badgeService.GetBadgesForUserAsync(userId!);
        return Ok(badges);
    }

    // GET: api/points/leaderboard?days=7
    [HttpGet("leaderboard")]
    public async Task<ActionResult<IEnumerable<LeaderboardEntryDto>>> GetLeaderboard([FromQuery] int days = 7)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);

        var leaderboard = await _context.PointsTransactions
            .Where(p => p.CreatedAt >= cutoff)
            .GroupBy(p => p.UserId)
            .Select(g => new { UserId = g.Key, TotalPoints = g.Sum(p => p.Points) })
            .OrderByDescending(x => x.TotalPoints)
            .Take(20)
            .ToListAsync();

        var userIds = leaderboard.Select(l => l.UserId).ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName);

        var result = leaderboard.Select((entry, index) => new LeaderboardEntryDto
        {
            Rank = index + 1,
            UserId = entry.UserId,
            DisplayName = users.GetValueOrDefault(entry.UserId, "Unknown"),
            Points = entry.TotalPoints
        }).ToList();

        return Ok(result);
    }
}