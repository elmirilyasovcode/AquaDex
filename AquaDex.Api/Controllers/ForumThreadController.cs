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
public class ForumThreadController : ControllerBase
{
    private readonly AquaDexDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ForumThreadController(AquaDexDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/forumthread?categoryId=1
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ForumThreadDto>>> GetThreads([FromQuery] int? categoryId)
    {
        var query = _context.ForumThreads
            .Include(t => t.Category)
            .Include(t => t.AuthorUser)
            .Include(t => t.Replies)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        var threads = await query
            .OrderByDescending(t => t.IsPinned)
            .ThenByDescending(t => t.CreatedAt)
            .Select(t => new ForumThreadDto
            {
                Id = t.Id,
                CategoryId = t.CategoryId,
                CategoryName = t.Category.Name,
                AuthorUserId = t.AuthorUserId,
                AuthorDisplayName = t.AuthorUser.DisplayName,
                Title = t.Title,
                CreatedAt = t.CreatedAt,
                IsPinned = t.IsPinned,
                ReplyCount = t.Replies.Count
            })
            .ToListAsync();

        return Ok(threads);
    }

    // GET: api/forumthread/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ForumThreadDetailDto>> GetThreadById(int id)
    {
        var thread = await _context.ForumThreads
            .Include(t => t.Category)
            .Include(t => t.AuthorUser)
            .Include(t => t.Replies).ThenInclude(r => r.AuthorUser)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (thread == null)
            return NotFound();

        var dto = new ForumThreadDetailDto
        {
            Id = thread.Id,
            CategoryId = thread.CategoryId,
            CategoryName = thread.Category.Name,
            AuthorUserId = thread.AuthorUserId,
            AuthorDisplayName = thread.AuthorUser.DisplayName,
            Title = thread.Title,
            Body = thread.Body,
            CreatedAt = thread.CreatedAt,
            IsPinned = thread.IsPinned,
            Replies = thread.Replies
                .OrderByDescending(r => r.IsBestAnswer)
                .ThenBy(r => r.CreatedAt)
                .Select(r => new ForumReplyDto
                {
                    Id = r.Id,
                    AuthorUserId = r.AuthorUserId,
                    AuthorDisplayName = r.AuthorUser.DisplayName,
                    Body = r.Body,
                    CreatedAt = r.CreatedAt,
                    IsBestAnswer = r.IsBestAnswer
                })
                .ToList()
        };

        return Ok(dto);
    }

    // POST: api/forumthread
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ForumThreadDetailDto>> CreateThread(CreateForumThreadDto dto)
    {
        var categoryExists = await _context.ForumCategories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
            return BadRequest($"Category with Id {dto.CategoryId} does not exist.");

        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var thread = new ForumThread
        {
            CategoryId = dto.CategoryId,
            AuthorUserId = userId,
            Title = dto.Title,
            Body = dto.Body,
            CreatedAt = DateTime.UtcNow
        };

        _context.ForumThreads.Add(thread);
        await _context.SaveChangesAsync();

        return await GetThreadById(thread.Id);
    }

    // POST: api/forumthread/5/reply
    [HttpPost("{id}/reply")]
    [Authorize]
    public async Task<ActionResult<ForumReplyDto>> AddReply(int id, CreateForumReplyDto dto)
    {
        var threadExists = await _context.ForumThreads.AnyAsync(t => t.Id == id);
        if (!threadExists)
            return NotFound($"Thread with Id {id} does not exist.");

        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var reply = new ForumReply
        {
            ThreadId = id,
            AuthorUserId = userId,
            Body = dto.Body,
            CreatedAt = DateTime.UtcNow
        };

        _context.ForumReplies.Add(reply);
        await _context.SaveChangesAsync();

        await _context.Entry(reply).Reference(r => r.AuthorUser).LoadAsync();

        return Ok(new ForumReplyDto
        {
            Id = reply.Id,
            AuthorUserId = reply.AuthorUserId,
            AuthorDisplayName = reply.AuthorUser.DisplayName,
            Body = reply.Body,
            CreatedAt = reply.CreatedAt,
            IsBestAnswer = reply.IsBestAnswer
        });
    }
}