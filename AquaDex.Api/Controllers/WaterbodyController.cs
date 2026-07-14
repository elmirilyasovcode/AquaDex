using AquaDex.Core.DTOs;
using AquaDex.Core.Entities;
using AquaDex.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaDex.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WaterbodyController : ControllerBase
{
    private readonly AquaDexDbContext _context;

    public WaterbodyController(AquaDexDbContext context)
    {
        _context = context;
    }

    // GET: api/waterbody
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WaterbodyDto>>> GetAllWaterbodies()
    {
        var waterbodies = await _context.Waterbodies
            .Select(w => new WaterbodyDto
            {
                Id = w.Id,
                Name = w.Name,
                Type = w.Type,
                Region = w.Region
            })
            .ToListAsync();

        return Ok(waterbodies);
    }

    // GET: api/waterbody/5
    [HttpGet("{id}")]
    public async Task<ActionResult<WaterbodyDto>> GetWaterbodyById(int id)
    {
        var waterbody = await _context.Waterbodies.FindAsync(id);

        if (waterbody == null)
            return NotFound();

        var dto = new WaterbodyDto
        {
            Id = waterbody.Id,
            Name = waterbody.Name,
            Type = waterbody.Type,
            Region = waterbody.Region
        };

        return Ok(dto);
    }

    // POST: api/waterbody
    [HttpPost]
    [Authorize(Roles = "VerifiedExpert,Admin")]
    public async Task<ActionResult<WaterbodyDto>> CreateWaterbody(CreateWaterbodyDto dto)
    {
        var waterbody = new Waterbody
        {
            Name = dto.Name,
            Type = dto.Type,
            Region = dto.Region
        };

        _context.Waterbodies.Add(waterbody);
        await _context.SaveChangesAsync();

        var resultDto = new WaterbodyDto
        {
            Id = waterbody.Id,
            Name = waterbody.Name,
            Type = waterbody.Type,
            Region = waterbody.Region
        };

        return CreatedAtAction(nameof(GetWaterbodyById), new { id = waterbody.Id }, resultDto);
    }
}