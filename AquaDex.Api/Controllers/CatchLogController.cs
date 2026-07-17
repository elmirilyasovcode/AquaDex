using AquaDex.Core.DTOs;
using AquaDex.Core.Entities;
using AquaDex.Core.Enums;
using AquaDex.Core.Helpers;
using AquaDex.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace AquaDex.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatchLogController : ControllerBase
{
    private readonly AquaDexDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CatchLogController(AquaDexDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/catchlog  (public feed — respects privacy toggle)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CatchLogDto>>> GetAllCatchLogs()
    {
        var logs = await _context.CatchLogs
            .Include(c => c.User)
            .Include(c => c.Species)
            .OrderByDescending(c => c.CaughtAt)
            .ToListAsync();

        var dtos = logs.Select(c => MapToDto(c)).ToList();
        return Ok(dtos);
    }

    // GET: api/catchlog/mine  (logged-in user's own catches — always full detail, own privacy doesn't hide from self)
    [HttpGet("mine")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CatchLogDto>>> GetMyCatchLogs()
    {
        var userId = _userManager.GetUserId(User);

        var logs = await _context.CatchLogs
            .Include(c => c.User)
            .Include(c => c.Species)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CaughtAt)
            .ToListAsync();

        // Own catches always show full location regardless of ShareExactLocation, since it's the owner viewing their own data
        var dtos = logs.Select(c => MapToDto(c, forceShowLocation: true)).ToList();
        return Ok(dtos);
    }

    // POST: api/catchlog
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CatchLogDto>> CreateCatchLog(CreateCatchLogDto dto)
    {
        var speciesExists = await _context.Species.AnyAsync(s => s.Id == dto.SpeciesId);
        if (!speciesExists)
            return BadRequest($"Species with Id {dto.SpeciesId} does not exist.");

        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var catchLog = new CatchLog
        {
            UserId = userId,
            SpeciesId = dto.SpeciesId,
            WeightKg = dto.WeightKg,
            LengthCm = dto.LengthCm,
            PhotoUrl = dto.PhotoUrl,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            ShareExactLocation = dto.ShareExactLocation,
            CaughtAt = dto.CaughtAt,
            CreatedAt = DateTime.UtcNow,
            Notes = dto.Notes
        };

        _context.CatchLogs.Add(catchLog);
        await _context.SaveChangesAsync();

        await _context.Entry(catchLog).Reference(c => c.User).LoadAsync();
        await _context.Entry(catchLog).Reference(c => c.Species).LoadAsync();

        return CreatedAtAction(nameof(GetAllCatchLogs), new { id = catchLog.Id }, MapToDto(catchLog, forceShowLocation: true));
    }

    private static CatchLogDto MapToDto(CatchLog c, bool forceShowLocation = false)
    {
        var showLocation = forceShowLocation || c.ShareExactLocation;

        return new CatchLogDto
        {
            Id = c.Id,
            UserId = c.UserId,
            UserDisplayName = c.User.DisplayName,
            SpeciesId = c.SpeciesId,
            SpeciesCommonNameEn = c.Species.CommonNameEn,
            WeightKg = c.WeightKg,
            LengthCm = c.LengthCm,
            PhotoUrl = c.PhotoUrl,
            Latitude = showLocation ? c.Latitude : null,
            Longitude = showLocation ? c.Longitude : null,
            ShareExactLocation = c.ShareExactLocation,
            CaughtAt = c.CaughtAt,
            CreatedAt = c.CreatedAt,
            Notes = c.Notes,
            IsProtectedSpeciesCatch = c.Species.ConservationStatus >= ConservationStatus.Vulnerable
        };
    }
    // GET: api/catchlog/nearby?latitude=40.4093&longitude=49.8671&radiusKm=25
    [HttpGet("nearby")]
    public async Task<ActionResult<IEnumerable<CatchLogDto>>> GetNearbyCatchLogs(
    [FromQuery] double latitude,
    [FromQuery] double longitude,
    [FromQuery] double radiusKm = 25)
    {
        var candidates = await _context.CatchLogs
            .Include(c => c.User)
            .Include(c => c.Species)
            .Where(c => c.Latitude != null && c.Longitude != null && c.ShareExactLocation)
            .ToListAsync();

        var nearby = GeoHelper.FilterByRadius(
            candidates,
            latitude,
            longitude,
            radiusKm,
            getLat: c => (double)c.Latitude!.Value,
            getLon: c => (double)c.Longitude!.Value
        );

        return Ok(nearby.Select(c => MapToDto(c, forceShowLocation: true)).ToList());
    }

    [HttpGet("my-discovered-species")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<int>>> GetMyDiscoveredSpeciesIds()
    {
        var userId = _userManager.GetUserId(User);

        var speciesIds = await _context.CatchLogs
            .Where(c => c.UserId == userId)
            .Select(c => c.SpeciesId)
            .Distinct()
            .ToListAsync();

        return Ok(speciesIds);
    }
}