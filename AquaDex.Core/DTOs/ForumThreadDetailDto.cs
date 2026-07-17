namespace AquaDex.Core.DTOs;

public class ForumThreadDetailDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string AuthorUserId { get; set; } = string.Empty;
    public string AuthorDisplayName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsPinned { get; set; }
    public List<ForumReplyDto> Replies { get; set; } = new();
}