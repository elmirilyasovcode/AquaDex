namespace AquaDex.Core.Entities;

public class ForumThread
{
    public int Id { get; set; }

    public int CategoryId { get; set; }
    public ForumCategory Category { get; set; } = null!;

    public string AuthorUserId { get; set; } = string.Empty;
    public ApplicationUser AuthorUser { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsPinned { get; set; } = false;

    public ICollection<ForumReply> Replies { get; set; } = new List<ForumReply>();
}