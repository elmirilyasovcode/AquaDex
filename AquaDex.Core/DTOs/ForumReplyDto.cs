namespace AquaDex.Core.DTOs;

public class ForumReplyDto
{
    public int Id { get; set; }
    public string AuthorUserId { get; set; } = string.Empty;
    public string AuthorDisplayName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsBestAnswer { get; set; }
    public int VoteCount { get; set; }
    public bool HasCurrentUserVoted { get; set; }
}