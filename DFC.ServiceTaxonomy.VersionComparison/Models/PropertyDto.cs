﻿using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.VersionComparison.Models
{
    public class PropertyDto
    {
        public string? Value { get; set; }
        public Dictionary<string, string>? Links { get; set; } 
        public string? Name { get; set; }
    }
}
