using AquaDex.Core.DTOs;
using AquaDex.Core.Entities;
using AquaDex.Core.Helpers;
using AquaDex.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaDex.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Asp.Versioning.ApiVersion("1.0")]
public class BiteAlertController : ControllerBase
{
    private readonly AquaDexDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    private const int AlertLifetimeHours = 3;

    public BiteAlertController(AquaDexDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/bitealert/nearby?latitude=..&longitude=..&radiusKm=25
    [HttpGet("nearby")]
    public async Task<ActionResult<IEnumerable<BiteAlertDto>>> GetNearbyActiveAlerts(
    [FromQuery] double latitude,
    [FromQuery] double longitude,
    [FromQuery] double radiusKm = 25)
    {
        var now = DateTime.UtcNow;

        var candidates = await _context.BiteAlerts
            .Include(b => b.PostedByUser)
            .Include(b => b.Species)
            .Where(b => b.ExpiresAt > now)
            .ToListAsync();

        var nearby = GeoHelper.FilterByRadius(
            candidates,
            latitude,
            longitude,
            radiusKm,
            getLat: b => (double)b.Latitude,
            getLon: b => (double)b.Longitude
        );

        return Ok(nearby.Select(MapToDto).ToList());
    }

    // POST: api/bitealert
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<BiteAlertDto>> CreateBiteAlert(CreateBiteAlertDto dto)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        if (dto.SpeciesId.HasValue)
        {
            var speciesExists = await _context.Species.AnyAsync(s => s.Id == dto.SpeciesId.Value);
            if (!speciesExists)
                return BadRequest($"Species with Id {dto.SpeciesId} does not exist.");
        }

        var now = DateTime.UtcNow;

        var alert = new BiteAlert
        {
            PostedByUserId = userId,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            SpeciesId = dto.SpeciesId,
            Message = dto.Message,
            PostedAt = now,
            ExpiresAt = now.AddHours(AlertLifetimeHours)
        };

        _context.BiteAlerts.Add(alert);
        await _context.SaveChangesAsync();

        await _context.Entry(alert).Reference(a => a.PostedByUser).LoadAsync();
        if (alert.SpeciesId.HasValue)
            await _context.Entry(alert).Reference(a => a.Species).LoadAsync();

        return CreatedAtAction(nameof(GetNearbyActiveAlerts), new { id = alert.Id }, MapToDto(alert));
    }

    private static BiteAlertDto MapToDto(BiteAlert b)
    {
        return new BiteAlertDto
        {
            Id = b.Id,
            PostedByDisplayName = b.PostedByUser.DisplayName,
            Latitude = b.Latitude,
            Longitude = b.Longitude,
            SpeciesId = b.SpeciesId,
            SpeciesCommonNameEn = b.Species?.CommonNameEn,
            Message = b.Message,
            PostedAt = b.PostedAt,
            ExpiresAt = b.ExpiresAt
        };
    }
}