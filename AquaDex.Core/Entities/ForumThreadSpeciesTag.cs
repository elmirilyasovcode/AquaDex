namespace AquaDex.Core.Entities;

public class ForumThreadSpeciesTag
{
    public int Id { get; set; }

    public int ThreadId { get; set; }
    public ForumThread Thread { get; set; } = null!;

    public int SpeciesId { get; set; }
    public Species Species { get; set; } = null!;
}