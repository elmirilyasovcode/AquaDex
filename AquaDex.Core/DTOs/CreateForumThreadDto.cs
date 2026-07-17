namespace AquaDex.Core.DTOs;

public class CreateForumThreadDto
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}