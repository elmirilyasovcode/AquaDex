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
public class ExpertConsultationController : ControllerBase
{
    private readonly AquaDexDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ExpertConsultationController(AquaDexDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/expertconsultation/experts  — list available experts to request from
    [HttpGet("experts")]
    public async Task<ActionResult<IEnumerable<object>>> GetAvailableExperts()
    {
        var experts = await _userManager.GetUsersInRoleAsync("VerifiedExpert");
        return Ok(experts.Select(e => new { userId = e.Id, displayName = e.DisplayName }));
    }

    // GET: api/expertconsultation/mine  — as requester
    [HttpGet("mine")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ExpertConsultationDto>>> GetMyRequests()
    {
        var userId = _userManager.GetUserId(User);
        var consultations = await _context.ExpertConsultations
            .Include(c => c.RequesterUser)
            .Include(c => c.ExpertUser)
            .Where(c => c.RequesterUserId == userId)
            .OrderByDescending(c => c.RequestedAt)
            .ToListAsync();

        return Ok(consultations.Select(MapToDto).ToList());
    }

    // GET: api/expertconsultation/incoming  — as expert
    [HttpGet("incoming")]
    [Authorize(Roles = "VerifiedExpert,Admin")]
    public async Task<ActionResult<IEnumerable<ExpertConsultationDto>>> GetIncomingRequests()
    {
        var userId = _userManager.GetUserId(User);
        var consultations = await _context.ExpertConsultations
            .Include(c => c.RequesterUser)
            .Include(c => c.ExpertUser)
            .Where(c => c.ExpertUserId == userId)
            .OrderByDescending(c => c.RequestedAt)
            .ToListAsync();

        return Ok(consultations.Select(MapToDto).ToList());
    }

    // POST: api/expertconsultation
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ExpertConsultationDto>> CreateRequest(CreateConsultationDto dto)
    {
        var expertExists = await _userManager.IsInRoleAsync(
            await _userManager.FindByIdAsync(dto.ExpertUserId) ?? new ApplicationUser(), "VerifiedExpert");

        var expertUser = await _userManager.FindByIdAsync(dto.ExpertUserId);
        if (expertUser == null || !await _userManager.IsInRoleAsync(expertUser, "VerifiedExpert"))
            return BadRequest("Selected user is not a Verified Expert.");

        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();

        var consultation = new ExpertConsultation
        {
            RequesterUserId = userId,
            ExpertUserId = dto.ExpertUserId,
            Subject = dto.Subject,
            Question = dto.Question,
            Status = ConsultationStatus.Requested,
            RequestedAt = DateTime.UtcNow
        };

        _context.ExpertConsultations.Add(consultation);
        await _context.SaveChangesAsync();

        await _context.Entry(consultation).Reference(c => c.RequesterUser).LoadAsync();
        await _context.Entry(consultation).Reference(c => c.ExpertUser).LoadAsync();

        return Ok(MapToDto(consultation));
    }

    // PATCH: api/expertconsultation/5/respond  — expert accepts/declines/completes
    [HttpPatch("{id}/respond")]
    [Authorize(Roles = "VerifiedExpert,Admin")]
    public async Task<ActionResult<ExpertConsultationDto>> Respond(int id, RespondToConsultationDto dto)
    {
        var consultation = await _context.ExpertConsultations
            .Include(c => c.RequesterUser)
            .Include(c => c.ExpertUser)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consultation == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (consultation.ExpertUserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        consultation.Status = dto.Status;
        consultation.ExpertResponse = dto.ExpertResponse;
        consultation.RespondedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToDto(consultation));
    }

    private static ExpertConsultationDto MapToDto(ExpertConsultation c)
    {
        return new ExpertConsultationDto
        {
            Id = c.Id,
            RequesterDisplayName = c.RequesterUser.DisplayName,
            ExpertUserId = c.ExpertUserId,
            ExpertDisplayName = c.ExpertUser.DisplayName,
            Subject = c.Subject,
            Question = c.Question,
            Status = c.Status.ToString(),
            RequestedAt = c.RequestedAt,
            RespondedAt = c.RespondedAt,
            ExpertResponse = c.ExpertResponse
        };
    }
}