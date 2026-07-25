using System.Text;
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
public class CatchLogController : ControllerBase
{
    private readonly AquaDexDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly PointsService _pointsService;
    public CatchLogController(AquaDexDbContext context, UserManager<ApplicationUser> userManager, PointsService pointsService)
    {
        _context = context;
        _userManager = userManager;
        _pointsService = pointsService;
    }

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

        var dtos = logs.Select(c => MapToDto(c, forceShowLocation: true)).ToList();
        return Ok(dtos);
    }

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

        await _pointsService.AwardPointsAsync(userId, PointsReason.CatchLogged, catchLog.Id);

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
    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportCsv()
    {
        var logs = await _context.CatchLogs
            .Include(c => c.User)
            .Include(c => c.Species)
            .OrderByDescending(c => c.CaughtAt)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Species,Angler,WeightKg,LengthCm,CaughtAt,Notes");

        foreach (var log in logs)
        {
            // Escape commas/quotes in free-text fields so the CSV doesn't break
            var notes = (log.Notes ?? "").Replace("\"", "\"\"");
            csv.AppendLine($"\"{log.Species.CommonNameEn}\",\"{log.User.DisplayName}\",{log.WeightKg},{log.LengthCm},{log.CaughtAt:yyyy-MM-dd},\"{notes}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"aquadex-catchlog-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf()
    {
        var logs = await _context.CatchLogs
            .Include(c => c.User)
            .Include(c => c.Species)
            .OrderByDescending(c => c.CaughtAt)
            .Take(50)
            .ToListAsync();

        var pdfBytes = AquaDex.Api.Services.CatchLogPdfBuilder.Build(logs);
        return File(pdfBytes, "application/pdf", $"aquadex-catchlog-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}