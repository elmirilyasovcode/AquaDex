using AquaDex.Core.DTOs;
using AquaDex.Core.Entities;
using AquaDex.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AquaDex.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Asp.Versioning.ApiVersion("1.0")]
public class WaterbodyController : ControllerBase
{
    private readonly AquaDexDbContext _context;
    private readonly IMemoryCache _cache;
    private const string WaterbodyListCacheKey = "waterbody:all";

    public WaterbodyController(AquaDexDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    // GET: api/waterbody
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WaterbodyDto>>> GetAllWaterbodies()
    {
        if (_cache.TryGetValue(WaterbodyListCacheKey, out List<WaterbodyDto>? cached))
        {
            return Ok(cached);
        }

        var waterbodies = await _context.Waterbodies
            .Select(w => new WaterbodyDto { Id = w.Id, Name = w.Name, Type = w.Type, Region = w.Region })
            .ToListAsync();

        _cache.Set(WaterbodyListCacheKey, waterbodies, TimeSpan.FromMinutes(10));

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
        _cache.Remove(WaterbodyListCacheKey);

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