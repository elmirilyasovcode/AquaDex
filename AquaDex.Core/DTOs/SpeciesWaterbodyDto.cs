namespace AquaDex.Core.DTOs;

public class SpeciesWaterbodyDto
{
    public int Id { get; set; }

    public int SpeciesId { get; set; }
    public string SpeciesCommonNameEn { get; set; } = string.Empty;

    public int WaterbodyId { get; set; }
    public string WaterbodyName { get; set; } = string.Empty;

    public int AbundanceRating { get; set; }
    public string SeasonNotes { get; set; } = string.Empty;
}