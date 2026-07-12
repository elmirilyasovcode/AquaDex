using AquaDex.Core.DTOs;
using AquaDex.Core.Entities;
using AquaDex.Core.Enums;
using AquaDex.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaDex.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpeciesController : ControllerBase
{
    private readonly AquaDexDbContext _context;

    public SpeciesController(AquaDexDbContext context)
    {
        _context = context;
    }

    // GET: api/species
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SpeciesDto>>> GetAllSpecies()
    {
        var species = await _context.Species
            .Select(s => new SpeciesDto
            {
                Id = s.Id,
                CommonNameAz = s.CommonNameAz,
                CommonNameEn = s.CommonNameEn,
                LatinName = s.LatinName,
                HabitatType = s.HabitatType,
                MinSizeCm = s.MinSizeCm,
                MaxSizeCm = s.MaxSizeCm,
                Diet = s.Diet,
                ConservationStatus = s.ConservationStatus,
                BestBaitTechnique = s.BestBaitTechnique,
                LegalSeasonNotes = s.LegalSeasonNotes,
                PhotoUrl = s.PhotoUrl
            })
            .ToListAsync();

        return Ok(species);
    }

    // GET: api/species/5
    [HttpGet("{id}")]
    public async Task<ActionResult<SpeciesDto>> GetSpeciesById(int id)
    {
        var species = await _context.Species.FindAsync(id);

        if (species == null)
            return NotFound();

        var dto = new SpeciesDto
        {
            Id = species.Id,
            CommonNameAz = species.CommonNameAz,
            CommonNameEn = species.CommonNameEn,
            LatinName = species.LatinName,
            HabitatType = species.HabitatType,
            MinSizeCm = species.MinSizeCm,
            MaxSizeCm = species.MaxSizeCm,
            Diet = species.Diet,
            ConservationStatus = species.ConservationStatus,
            BestBaitTechnique = species.BestBaitTechnique,
            LegalSeasonNotes = species.LegalSeasonNotes,
            PhotoUrl = species.PhotoUrl
        };

        return Ok(dto);
    }

    // POST: api/species
    [HttpPost]
    public async Task<ActionResult<SpeciesDto>> CreateSpecies(CreateSpeciesDto dto)
    {
        var species = new Species
        {
            CommonNameAz = dto.CommonNameAz,
            CommonNameEn = dto.CommonNameEn,
            LatinName = dto.LatinName,
            HabitatType = dto.HabitatType,
            MinSizeCm = dto.MinSizeCm,
            MaxSizeCm = dto.MaxSizeCm,
            Diet = dto.Diet,
            ConservationStatus = dto.ConservationStatus,
            BestBaitTechnique = dto.BestBaitTechnique,
            LegalSeasonNotes = dto.LegalSeasonNotes,
            PhotoUrl = dto.PhotoUrl
        };

        _context.Species.Add(species);
        await _context.SaveChangesAsync();

        var resultDto = new SpeciesDto
        {
            Id = species.Id,
            CommonNameAz = species.CommonNameAz,
            CommonNameEn = species.CommonNameEn,
            LatinName = species.LatinName,
            HabitatType = species.HabitatType,
            MinSizeCm = species.MinSizeCm,
            MaxSizeCm = species.MaxSizeCm,
            Diet = species.Diet,
            ConservationStatus = species.ConservationStatus,
            BestBaitTechnique = species.BestBaitTechnique,
            LegalSeasonNotes = species.LegalSeasonNotes,
            PhotoUrl = species.PhotoUrl
        };

        return CreatedAtAction(nameof(GetSpeciesById), new { id = species.Id }, resultDto);
    }

    // GET: api/species/search?habitatType=1&conservationStatus=4&nameContains=stur
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<SpeciesDto>>> SearchSpecies(
        [FromQuery] HabitatType? habitatType,
        [FromQuery] ConservationStatus? conservationStatus,
        [FromQuery] string? nameContains)
    {
        var query = _context.Species.AsQueryable();

        if (habitatType.HasValue)
            query = query.Where(s => s.HabitatType == habitatType.Value);

        if (conservationStatus.HasValue)
            query = query.Where(s => s.ConservationStatus == conservationStatus.Value);

        if (!string.IsNullOrWhiteSpace(nameContains))
            query = query.Where(s =>
                s.CommonNameEn.Contains(nameContains) ||
                s.CommonNameAz.Contains(nameContains) ||
                s.LatinName.Contains(nameContains));

        var results = await query
            .Select(s => new SpeciesDto
            {
                Id = s.Id,
                CommonNameAz = s.CommonNameAz,
                CommonNameEn = s.CommonNameEn,
                LatinName = s.LatinName,
                HabitatType = s.HabitatType,
                MinSizeCm = s.MinSizeCm,
                MaxSizeCm = s.MaxSizeCm,
                Diet = s.Diet,
                ConservationStatus = s.ConservationStatus,
                BestBaitTechnique = s.BestBaitTechnique,
                LegalSeasonNotes = s.LegalSeasonNotes,
                PhotoUrl = s.PhotoUrl
            })
            .ToListAsync();

        return Ok(results);
    }
}