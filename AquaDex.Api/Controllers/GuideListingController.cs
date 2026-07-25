using AquaDex.Core.DTOs;
using AquaDex.Core.Entities;
using AquaDex.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaDex.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Asp.Versioning.ApiVersion("1.0")]
public class GuideListingController : ControllerBase
{
    private readonly AquaDexDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public GuideListingController(AquaDexDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/guidelisting?region=Quba
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GuideListingDto>>> GetListings([FromQuery] string? region)
    {
        var query = _context.GuideListings.Include(g => g.GuideUser).AsQueryable();

        if (!string.IsNullOrWhiteSpace(region))
            query = query.Where(g => g.Region.Contains(region));

        var listings = await query.OrderByDescending(g => g.CreatedAt).ToListAsync();

        return Ok(listings.Select(g => new GuideListingDto
        {
            Id = g.Id,
            GuideUserId = g.GuideUserId,
            GuideDisplayName = g.GuideUser.DisplayName,
            Title = g.Title,
            Description = g.Description,
            Region = g.Region,
            PricePerDay = g.PricePerDay,
            CreatedAt = g.CreatedAt
        }).ToList());
    }

    // POST: api/guidelisting
    [HttpPost]
    [Authorize(Roles = "FishingGuide,Admin")]
    public async Task<ActionResult<GuideListingDto>> CreateListing(CreateGuideListingDto dto)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();

        var listing = new GuideListing
        {
            GuideUserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            Region = dto.Region,
            PricePerDay = dto.PricePerDay,
            CreatedAt = DateTime.UtcNow
        };

        _context.GuideListings.Add(listing);
        await _context.SaveChangesAsync();
        await _context.Entry(listing).Reference(g => g.GuideUser).LoadAsync();

        return Ok(new GuideListingDto
        {
            Id = listing.Id,
            GuideUserId = listing.GuideUserId,
            GuideDisplayName = listing.GuideUser.DisplayName,
            Title = listing.Title,
            Description = listing.Description,
            Region = listing.Region,
            PricePerDay = listing.PricePerDay,
            CreatedAt = listing.CreatedAt
        });
    }
}