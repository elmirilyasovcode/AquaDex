namespace AquaDex.Core.DTOs;

public class ForumCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public int ThreadCount { get; set; }
}