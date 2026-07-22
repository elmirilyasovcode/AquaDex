using System.Runtime.InteropServices;
using AquaDex.Core.DTOs;
using AquaDex.Core.Entities;
using AquaDex.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaDex.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserNotificationController : ControllerBase
{
    private readonly AquaDexDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserNotificationController(AquaDexDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/usernotification
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserNotificationDto>>> GetMyNotifications()
    {
        var userId = _userManager.GetUserId(User);
        var notifications = await _context.UserNotifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(20)
            .ToListAsync();

        return Ok(notifications.Select(n => new UserNotificationDto
        {
            Id = n.Id,
            Message = n.Message,
            Type = n.Type.ToString(),
            CreatedAt = n.CreatedAt,
            IsRead = n.IsRead
        }).ToList());
    }

    // PATCH: api/usernotification/5/read
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = _userManager.GetUserId(User);
        var notification = await _context.UserNotifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (notification == null) return NotFound();

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return Ok();
    }
}