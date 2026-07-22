namespace AquaDex.Core.Entities;

public class ForumThreadWaterbodyTag
{
    public int Id { get; set; }

    public int ThreadId { get; set; }
    public ForumThread Thread { get; set; } = null!;

    public int WaterbodyId { get; set; }
    public Waterbody Waterbody { get; set; } = null!;
}