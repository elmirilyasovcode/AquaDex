using System;
using System.Collections.Generic;
using System.Text;

namespace AquaDex.Core.Entities
{
    public class SpeciesWaterbody
    {
        public int Id { get; set; }

        public int SpeciesId { get; set; }
        public Species Species { get; set; } = null!;

        public int WaterbodyId { get; set; }
        public Waterbody Waterbody { get; set; } = null!;

        public int AbundanceRating { get; set; } // 1-5
        public string SeasonNotes { get; set; } = string.Empty;
    }
}
