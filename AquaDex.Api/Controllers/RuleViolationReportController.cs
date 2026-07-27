using AquaDex.Core.DTOs;
using AquaDex.Core.Entities;
using AquaDex.Core.Enums;
using AquaDex.Core.Helpers;
using AquaDex.Infrastructure.Data;
using AquaDex.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaDex.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Asp.Versioning.ApiVersion("1.0")]
public class RuleViolationReportController : ControllerBase
{
    private readonly AquaDexDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AuditService _auditService;

    public RuleViolationReportController(AquaDexDbContext context, UserManager<ApplicationUser> userManager, AuditService auditService)
    {
        _context = context;
        _userManager = userManager;
        _auditService = auditService;
    }

        [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<RuleViolationReportDto>>> GetAllReports()
    {
        var reports = await _context.RuleViolationReports
            .Include(r => r.ReportedByUser)
            .Include(r => r.Waterbody)
            .OrderByDescending(r => r.ReportedAt)
            .ToListAsync();

        return Ok(reports.Select(MapToDto).ToList());
    }

            [HttpGet("nearby")]
    public async Task<ActionResult<IEnumerable<RuleViolationReportDto>>> GetNearbyReports(
    [FromQuery] double latitude,
    [FromQuery] double longitude,
    [FromQuery] double radiusKm = 25)
    {
        var candidates = await _context.RuleViolationReports
            .Include(r => r.ReportedByUser)
            .Include(r => r.Waterbody)
            .Where(r => r.Status != ReportStatus.Dismissed)
            .ToListAsync();

        var nearby = GeoHelper.FilterByRadius(
            candidates,
            latitude,
            longitude,
            radiusKm,
            getLat: r => (double)r.Latitude,
            getLon: r => (double)r.Longitude
        );

        return Ok(nearby.Select(MapToDto).ToList());
    }

        [HttpPost]
    [Authorize]
    public async Task<ActionResult<RuleViolationReportDto>> CreateReport(CreateRuleViolationReportDto dto)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        if (dto.WaterbodyId.HasValue)
        {
            var waterbodyExists = await _context.Waterbodies.AnyAsync(w => w.Id == dto.WaterbodyId.Value);
            if (!waterbodyExists)
                return BadRequest($"Waterbody with Id {dto.WaterbodyId} does not exist.");
        }

        var report = new RuleViolationReport
        {
            ReportedByUserId = userId,
            ViolationType = dto.ViolationType,
            Description = dto.Description,
            PhotoUrl = dto.PhotoUrl,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            WaterbodyId = dto.WaterbodyId,
            Status = ReportStatus.Pending,
            ReportedAt = DateTime.UtcNow
        };

        _context.RuleViolationReports.Add(report);
        await _context.SaveChangesAsync();

        await _context.Entry(report).Reference(r => r.ReportedByUser).LoadAsync();
        if (report.WaterbodyId.HasValue)
            await _context.Entry(report).Reference(r => r.Waterbody).LoadAsync();

        return CreatedAtAction(nameof(GetAllReports), new { id = report.Id }, MapToDto(report));
    }

        [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RuleViolationReportDto>> UpdateStatus(int id, [FromBody] ReportStatus newStatus)
    {
        var report = await _context.RuleViolationReports
            .Include(r => r.ReportedByUser)
            .Include(r => r.Waterbody)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null)
            return NotFound();

        report.Status = newStatus;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            _userManager.GetUserId(User)!,
            "RuleViolationReport.StatusChanged",
            "RuleViolationReport",
            id.ToString(),
            $"New status: {newStatus}"
        );

        return Ok(MapToDto(report));
    }

    private static RuleViolationReportDto MapToDto(RuleViolationReport r)
    {
        return new RuleViolationReportDto
        {
            Id = r.Id,
            ReportedByUserId = r.ReportedByUserId,
            ReportedByDisplayName = r.ReportedByUser.DisplayName,
            ViolationType = r.ViolationType,
            Description = r.Description,
            PhotoUrl = r.PhotoUrl,
            Latitude = r.Latitude,
            Longitude = r.Longitude,
            WaterbodyId = r.WaterbodyId,
            WaterbodyName = r.Waterbody?.Name,
            Status = r.Status,
            ReportedAt = r.ReportedAt
        };
    }
}