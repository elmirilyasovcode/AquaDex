using AquaDex.Core.DTOs;
using AquaDex.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaDex.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForumCategoryController : ControllerBase
{
    private readonly AquaDexDbContext _context;

    public ForumCategoryController(AquaDexDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ForumCategoryDto>>> GetAllCategories()
    {
        var categories = await _context.ForumCategories
            .Include(c => c.Threads)
            .OrderBy(c => c.SortOrder)
            .Select(c => new ForumCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                SortOrder = c.SortOrder,
                ThreadCount = c.Threads.Count
            })
            .ToListAsync();

        return Ok(categories);
    }
}