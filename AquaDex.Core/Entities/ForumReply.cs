namespace AquaDex.Core.Entities;

public class ForumReply
{
    public int Id { get; set; }

    public int ThreadId { get; set; }
    public ForumThread Thread { get; set; } = null!;

    public string AuthorUserId { get; set; } = string.Empty;
    public ApplicationUser AuthorUser { get; set; } = null!;

    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsBestAnswer { get; set; } = false;
}