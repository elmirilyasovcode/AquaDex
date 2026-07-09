using System;
using System.Collections.Generic;
using System.Text;
using AquaDex.Core.Enums;

namespace AquaDex.Core.DTOs
{
    public class CreateSpeciesDto
    {
        public string CommonNameAz { get; set; } = string.Empty;
        public string CommonNameEn { get; set; } = string.Empty;
        public string LatinName { get; set; } = string.Empty;
        public HabitatType HabitatType { get; set; }
        public decimal MinSizeCm { get; set; }
        public decimal MaxSizeCm { get; set; }
        public string Diet { get; set; } = string.Empty;
        public ConservationStatus ConservationStatus { get; set; }
        public string BestBaitTechnique { get; set; } = string.Empty;
        public string LegalSeasonNotes { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
    }
}
