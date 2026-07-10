using System;
using System.Collections.Generic;
using System.Text;

namespace AquaDex.Core.DTOs;

public class CreateSpeciesWaterbodyDto
{
    public int SpeciesId { get; set; }
    public int WaterbodyId { get; set; }
    public int AbundanceRating { get; set; }
    public string SeasonNotes { get; set; } = string.Empty;
}
