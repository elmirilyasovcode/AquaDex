using AquaDex.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaDex.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Asp.Versioning.ApiVersion("1.0")]
[Authorize(Roles = "Admin")]
public class AuditLogController : ControllerBase
{
    private readonly AquaDexDbContext _context;

    public AuditLogController(AquaDexDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecent()
    {
        var entries = await _context.AuditLogEntries
            .OrderByDescending(e => e.Timestamp)
            .Take(100)
            .ToListAsync();

        return Ok(entries);
    }
}