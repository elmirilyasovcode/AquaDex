namespace AquaDex.Core.Entities;

public class ForumReplyVote
{
    public int Id { get; set; }

    public int ReplyId { get; set; }
    public ForumReply Reply { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}