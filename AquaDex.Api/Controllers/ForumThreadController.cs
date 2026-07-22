using AquaDex.Api.Hubs;
using AquaDex.Core.DTOs;
using AquaDex.Core.Entities;
using AquaDex.Core.Enums;
using AquaDex.Infrastructure.Data;
using AquaDex.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AquaDex.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForumThreadController : ControllerBase
{
    private readonly AquaDexDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly PointsService _pointsService;
    private readonly IHubContext<ForumHub> _hubContext;

    public ForumThreadController(AquaDexDbContext context, UserManager<ApplicationUser> userManager, PointsService pointsService, IHubContext<ForumHub> hubContext)
    {
        _context = context;
        _userManager = userManager;
        _pointsService = pointsService;
        _hubContext = hubContext;
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
            .ToListAsync();

        var threadIds = threads.Select(t => t.Id).ToList();

        var speciesTags = await _context.ForumThreadSpeciesTags
            .Include(t => t.Species)
            .Where(t => threadIds.Contains(t.ThreadId))
            .ToListAsync();

        var waterbodyTags = await _context.ForumThreadWaterbodyTags
            .Include(t => t.Waterbody)
            .Where(t => threadIds.Contains(t.ThreadId))
            .ToListAsync();

        var result = threads.Select(t => new ForumThreadDto
        {
            Id = t.Id,
            CategoryId = t.CategoryId,
            CategoryName = t.Category.Name,
            AuthorUserId = t.AuthorUserId,
            AuthorDisplayName = t.AuthorUser.DisplayName,
            Title = t.Title,
            CreatedAt = t.CreatedAt,
            IsPinned = t.IsPinned,
            ReplyCount = t.Replies.Count,
            SpeciesTags = speciesTags.Where(st => st.ThreadId == t.Id)
                .Select(st => new TagSummaryDto { Id = st.Species.Id, Name = st.Species.CommonNameEn })
                .ToList(),
            WaterbodyTags = waterbodyTags.Where(wt => wt.ThreadId == t.Id)
                .Select(wt => new TagSummaryDto { Id = wt.Waterbody.Id, Name = wt.Waterbody.Name })
                .ToList()
        }).ToList();

        return Ok(result);
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

        var speciesTags = await _context.ForumThreadSpeciesTags
            .Include(t => t.Species)
            .Where(t => t.ThreadId == id)
            .Select(t => new TagSummaryDto { Id = t.Species.Id, Name = t.Species.CommonNameEn })
            .ToListAsync();

        var waterbodyTags = await _context.ForumThreadWaterbodyTags
            .Include(t => t.Waterbody)
            .Where(t => t.ThreadId == id)
            .Select(t => new TagSummaryDto { Id = t.Waterbody.Id, Name = t.Waterbody.Name })
            .ToListAsync();

        var currentUserId = _userManager.GetUserId(User); // null if not logged in — that's fine

        var replyIds = thread.Replies.Select(r => r.Id).ToList();
        var voteCounts = await _context.ForumReplyVotes
            .Where(v => replyIds.Contains(v.ReplyId))
            .GroupBy(v => v.ReplyId)
            .Select(g => new { ReplyId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ReplyId, x => x.Count);

        var myVotedReplyIds = currentUserId == null
            ? new HashSet<int>()
            : (await _context.ForumReplyVotes
                .Where(v => replyIds.Contains(v.ReplyId) && v.UserId == currentUserId)
                .Select(v => v.ReplyId)
                .ToListAsync()).ToHashSet();

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
            SpeciesTags = speciesTags,
            WaterbodyTags = waterbodyTags,
            Replies = thread.Replies
        .OrderByDescending(r => r.IsBestAnswer)
        .ThenByDescending(r => voteCounts.GetValueOrDefault(r.Id, 0))
        .ThenBy(r => r.CreatedAt)
        .Select(r => new ForumReplyDto
        {
            Id = r.Id,
            AuthorUserId = r.AuthorUserId,
            AuthorDisplayName = r.AuthorUser.DisplayName,
            Body = r.Body,
            CreatedAt = r.CreatedAt,
            IsBestAnswer = r.IsBestAnswer,
            VoteCount = voteCounts.GetValueOrDefault(r.Id, 0),
            HasCurrentUserVoted = myVotedReplyIds.Contains(r.Id)
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

        // Validate tagged species/waterbodies actually exist before committing
        if (dto.SpeciesTagIds.Any())
        {
            var validSpeciesCount = await _context.Species.CountAsync(s => dto.SpeciesTagIds.Contains(s.Id));
            if (validSpeciesCount != dto.SpeciesTagIds.Distinct().Count())
                return BadRequest("One or more tagged Species do not exist.");
        }

        if (dto.WaterbodyTagIds.Any())
        {
            var validWaterbodyCount = await _context.Waterbodies.CountAsync(w => dto.WaterbodyTagIds.Contains(w.Id));
            if (validWaterbodyCount != dto.WaterbodyTagIds.Distinct().Count())
                return BadRequest("One or more tagged Waterbodies do not exist.");
        }

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

        await _pointsService.AwardPointsAsync(userId, PointsReason.ForumThreadPosted, thread.Id);

        foreach (var speciesId in dto.SpeciesTagIds.Distinct())
        {
            _context.ForumThreadSpeciesTags.Add(new ForumThreadSpeciesTag { ThreadId = thread.Id, SpeciesId = speciesId });
        }

        foreach (var waterbodyId in dto.WaterbodyTagIds.Distinct())
        {
            _context.ForumThreadWaterbodyTags.Add(new ForumThreadWaterbodyTag { ThreadId = thread.Id, WaterbodyId = waterbodyId });
        }

        if (dto.SpeciesTagIds.Any() || dto.WaterbodyTagIds.Any())
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
        await _pointsService.AwardPointsAsync(userId, PointsReason.ForumReplyPosted, reply.Id);

        var replyDto = new ForumReplyDto
        {
            Id = reply.Id,
            AuthorUserId = reply.AuthorUserId,
            AuthorDisplayName = reply.AuthorUser.DisplayName,
            Body = reply.Body,
            CreatedAt = reply.CreatedAt,
            IsBestAnswer = reply.IsBestAnswer,
            VoteCount = 0,
            HasCurrentUserVoted = false
        };

        await _hubContext.Clients.Group($"thread-{id}").SendAsync("ReceiveReply", replyDto);

        return Ok(replyDto);

        
    }
    // POST: api/forumthread/reply/5/vote
    [HttpPost("reply/{replyId}/vote")]
    [Authorize]
    public async Task<IActionResult> VoteOnReply(int replyId)
    {
        var replyExists = await _context.ForumReplies.AnyAsync(r => r.Id == replyId);
        if (!replyExists)
            return NotFound($"Reply with Id {replyId} does not exist.");

        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var alreadyVoted = await _context.ForumReplyVotes
            .AnyAsync(v => v.ReplyId == replyId && v.UserId == userId);

        if (alreadyVoted)
            return Conflict("You have already voted on this reply.");

        _context.ForumReplyVotes.Add(new ForumReplyVote
        {
            ReplyId = replyId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return Ok();
    }

    // DELETE: api/forumthread/reply/5/vote  (un-vote)
    [HttpDelete("reply/{replyId}/vote")]
    [Authorize]
    public async Task<IActionResult> RemoveVote(int replyId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var vote = await _context.ForumReplyVotes
            .FirstOrDefaultAsync(v => v.ReplyId == replyId && v.UserId == userId);

        if (vote == null)
            return NotFound("You have not voted on this reply.");

        _context.ForumReplyVotes.Remove(vote);
        await _context.SaveChangesAsync();
        return Ok();
    }

    // POST: api/forumthread/5/reply/10/mark-best  (thread author or Admin only)
    [HttpPost("{threadId}/reply/{replyId}/mark-best")]
    [Authorize]
    public async Task<IActionResult> MarkBestAnswer(int threadId, int replyId)
    {
        var thread = await _context.ForumThreads
            .Include(t => t.Replies)
            .FirstOrDefaultAsync(t => t.Id == threadId);

        if (thread == null)
            return NotFound($"Thread with Id {threadId} does not exist.");

        var userId = _userManager.GetUserId(User);
        var isAdmin = User.IsInRole("Admin");

        // Only the thread's original author (or an Admin) can mark a best answer —
        // otherwise anyone could pick winners on someone else's question
        if (thread.AuthorUserId != userId && !isAdmin)
            return Forbid();

        var targetReply = thread.Replies.FirstOrDefault(r => r.Id == replyId);
        if (targetReply == null)
            return NotFound($"Reply with Id {replyId} does not exist on this thread.");

        // Clear any existing best-answer flag on this thread first — only one allowed at a time
        foreach (var reply in thread.Replies)
        {
            reply.IsBestAnswer = (reply.Id == replyId);
        }

        await _context.SaveChangesAsync();

        await _pointsService.AwardPointsAsync(targetReply.AuthorUserId, PointsReason.ForumBestAnswer, targetReply.Id);
        return Ok();
    }
    // GET: api/forumthread/by-species/5
    [HttpGet("by-species/{speciesId}")]
    public async Task<ActionResult<IEnumerable<ForumThreadDto>>> GetThreadsBySpecies(int speciesId)
    {
        var threadIds = await _context.ForumThreadSpeciesTags
            .Where(t => t.SpeciesId == speciesId)
            .Select(t => t.ThreadId)
            .ToListAsync();

        var threads = await _context.ForumThreads
            .Include(t => t.Category)
            .Include(t => t.AuthorUser)
            .Include(t => t.Replies)
            .Where(t => threadIds.Contains(t.Id))
            .OrderByDescending(t => t.CreatedAt)
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

    // GET: api/forumthread/by-waterbody/5
    [HttpGet("by-waterbody/{waterbodyId}")]
    public async Task<ActionResult<IEnumerable<ForumThreadDto>>> GetThreadsByWaterbody(int waterbodyId)
    {
        var threadIds = await _context.ForumThreadWaterbodyTags
            .Where(t => t.WaterbodyId == waterbodyId)
            .Select(t => t.ThreadId)
            .ToListAsync();

        var threads = await _context.ForumThreads
            .Include(t => t.Category)
            .Include(t => t.AuthorUser)
            .Include(t => t.Replies)
            .Where(t => threadIds.Contains(t.Id))
            .OrderByDescending(t => t.CreatedAt)
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
}