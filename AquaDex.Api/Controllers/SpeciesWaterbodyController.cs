using AquaDex.Core.DTOs;
using AquaDex.Core.Entities;
using AquaDex.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaDex.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpeciesWaterbodyController : ControllerBase
{
    private readonly AquaDexDbContext _context;

    public SpeciesWaterbodyController(AquaDexDbContext context)
    {
        _context = context;
    }

    // GET: api/specieswaterbody
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SpeciesWaterbodyDto>>> GetAllLinks()
    {
        var links = await _context.SpeciesWaterbodies
            .Include(sw => sw.Species)
            .Include(sw => sw.Waterbody)
            .Select(sw => new SpeciesWaterbodyDto
            {
                Id = sw.Id,
                SpeciesId = sw.SpeciesId,
                SpeciesCommonNameEn = sw.Species.CommonNameEn,
                WaterbodyId = sw.WaterbodyId,
                WaterbodyName = sw.Waterbody.Name,
                AbundanceRating = sw.AbundanceRating,
                SeasonNotes = sw.SeasonNotes
            })
            .ToListAsync();

        return Ok(links);
    }

    // GET: api/specieswaterbody/by-waterbody/5
    // All species found in a specific waterbody — this is the "what fish live here" query
    [HttpGet("by-waterbody/{waterbodyId}")]
    public async Task<ActionResult<IEnumerable<SpeciesWaterbodyDto>>> GetByWaterbody(int waterbodyId)
    {
        var links = await _context.SpeciesWaterbodies
            .Include(sw => sw.Species)
            .Include(sw => sw.Waterbody)
            .Where(sw => sw.WaterbodyId == waterbodyId)
            .Select(sw => new SpeciesWaterbodyDto
            {
                Id = sw.Id,
                SpeciesId = sw.SpeciesId,
                SpeciesCommonNameEn = sw.Species.CommonNameEn,
                WaterbodyId = sw.WaterbodyId,
                WaterbodyName = sw.Waterbody.Name,
                AbundanceRating = sw.AbundanceRating,
                SeasonNotes = sw.SeasonNotes
            })
            .ToListAsync();

        return Ok(links);
    }

    // GET: api/specieswaterbody/by-species/5
    // All waterbodies where a specific species is found — the reverse query
    [HttpGet("by-species/{speciesId}")]
    public async Task<ActionResult<IEnumerable<SpeciesWaterbodyDto>>> GetBySpecies(int speciesId)
    {
        var links = await _context.SpeciesWaterbodies
            .Include(sw => sw.Species)
            .Include(sw => sw.Waterbody)
            .Where(sw => sw.SpeciesId == speciesId)
            .Select(sw => new SpeciesWaterbodyDto
            {
                Id = sw.Id,
                SpeciesId = sw.SpeciesId,
                SpeciesCommonNameEn = sw.Species.CommonNameEn,
                WaterbodyId = sw.WaterbodyId,
                WaterbodyName = sw.Waterbody.Name,
                AbundanceRating = sw.AbundanceRating,
                SeasonNotes = sw.SeasonNotes
            })
            .ToListAsync();

        return Ok(links);
    }

    // POST: api/specieswaterbody
    [HttpPost]
    public async Task<ActionResult<SpeciesWaterbodyDto>> CreateLink(CreateSpeciesWaterbodyDto dto)
    {
        // Validate both FKs actually exist before inserting —
        // otherwise EF throws a raw DB foreign-key error, which is a bad experience for API consumers
        var speciesExists = await _context.Species.AnyAsync(s => s.Id == dto.SpeciesId);
        if (!speciesExists)
            return BadRequest($"Species with Id {dto.SpeciesId} does not exist.");

        var waterbodyExists = await _context.Waterbodies.AnyAsync(w => w.Id == dto.WaterbodyId);
        if (!waterbodyExists)
            return BadRequest($"Waterbody with Id {dto.WaterbodyId} does not exist.");

        // Check for an existing duplicate pair before hitting the DB's unique index —
        // lets us return a clean 409 Conflict instead of an ugly SQL exception
        var duplicateExists = await _context.SpeciesWaterbodies
            .AnyAsync(sw => sw.SpeciesId == dto.SpeciesId && sw.WaterbodyId == dto.WaterbodyId);
        if (duplicateExists)
            return Conflict($"A link between Species {dto.SpeciesId} and Waterbody {dto.WaterbodyId} already exists.");

        var link = new SpeciesWaterbody
        {
            SpeciesId = dto.SpeciesId,
            WaterbodyId = dto.WaterbodyId,
            AbundanceRating = dto.AbundanceRating,
            SeasonNotes = dto.SeasonNotes
        };

        _context.SpeciesWaterbodies.Add(link);
        await _context.SaveChangesAsync();

        // Reload with navigation properties populated so the response DTO has real names, not blanks
        await _context.Entry(link).Reference(l => l.Species).LoadAsync();
        await _context.Entry(link).Reference(l => l.Waterbody).LoadAsync();

        var resultDto = new SpeciesWaterbodyDto
        {
            Id = link.Id,
            SpeciesId = link.SpeciesId,
            SpeciesCommonNameEn = link.Species.CommonNameEn,
            WaterbodyId = link.WaterbodyId,
            WaterbodyName = link.Waterbody.Name,
            AbundanceRating = link.AbundanceRating,
            SeasonNotes = link.SeasonNotes
        };

        return CreatedAtAction(nameof(GetAllLinks), new { id = link.Id }, resultDto);
    }
}