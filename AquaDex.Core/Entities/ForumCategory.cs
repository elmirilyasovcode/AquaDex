namespace AquaDex.Core.Entities;

public class ForumCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public ICollection<ForumThread> Threads { get; set; } = new List<ForumThread>();
}