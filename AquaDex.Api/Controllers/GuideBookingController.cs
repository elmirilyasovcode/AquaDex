using AquaDex.Core.DTOs;
using AquaDex.Core.Entities;
using AquaDex.Core.Enums;
using AquaDex.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaDex.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GuideBookingController : ControllerBase
{
    private readonly AquaDexDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public GuideBookingController(AquaDexDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/guidebooking/mine  — as requester
    [HttpGet("mine")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<GuideBookingDto>>> GetMyBookings()
    {
        var userId = _userManager.GetUserId(User);
        var bookings = await _context.GuideBookings
            .Include(b => b.Listing)
            .Include(b => b.RequesterUser)
            .Where(b => b.RequesterUserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return Ok(bookings.Select(MapToDto).ToList());
    }

    // GET: api/guidebooking/incoming  — as the guide who owns the listing
    [HttpGet("incoming")]
    [Authorize(Roles = "FishingGuide,Admin")]
    public async Task<ActionResult<IEnumerable<GuideBookingDto>>> GetIncomingBookings()
    {
        var userId = _userManager.GetUserId(User);
        var bookings = await _context.GuideBookings
            .Include(b => b.Listing)
            .Include(b => b.RequesterUser)
            .Where(b => b.Listing.GuideUserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return Ok(bookings.Select(MapToDto).ToList());
    }

    // POST: api/guidebooking
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<GuideBookingDto>> CreateBooking(CreateGuideBookingDto dto)
    {
        var listing = await _context.GuideListings.FindAsync(dto.ListingId);
        if (listing == null)
            return BadRequest($"Listing with Id {dto.ListingId} does not exist.");

        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();

        var booking = new GuideBooking
        {
            ListingId = dto.ListingId,
            RequesterUserId = userId,
            RequestedDate = dto.RequestedDate,
            Notes = dto.Notes,
            Status = BookingStatus.Requested,
            CreatedAt = DateTime.UtcNow
        };

        _context.GuideBookings.Add(booking);
        await _context.SaveChangesAsync();

        await _context.Entry(booking).Reference(b => b.Listing).LoadAsync();
        await _context.Entry(booking).Reference(b => b.RequesterUser).LoadAsync();

        return Ok(MapToDto(booking));
    }

    // PATCH: api/guidebooking/5/status  — guide confirms/declines
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "FishingGuide,Admin")]
    public async Task<ActionResult<GuideBookingDto>> UpdateStatus(int id, [FromBody] BookingStatus newStatus)
    {
        var booking = await _context.GuideBookings
            .Include(b => b.Listing)
            .Include(b => b.RequesterUser)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (booking.Listing.GuideUserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        booking.Status = newStatus;
        booking.RespondedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToDto(booking));
    }

    private static GuideBookingDto MapToDto(GuideBooking b)
    {
        return new GuideBookingDto
        {
            Id = b.Id,
            ListingId = b.ListingId,
            ListingTitle = b.Listing.Title,
            RequesterDisplayName = b.RequesterUser.DisplayName,
            RequestedDate = b.RequestedDate,
            Status = b.Status.ToString(),
            Notes = b.Notes,
            CreatedAt = b.CreatedAt
        };
    }
}