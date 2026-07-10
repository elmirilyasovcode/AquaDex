using System;
using System.Collections.Generic;
using System.Text;
using AquaDex.Core.Enums;

namespace AquaDex.Core.DTOs
{
    public class WaterbodyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public WaterbodyType Type { get; set; }
        public string Region { get; set; } = string.Empty;
    }
}
