using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.VersionComparison.Models
{
    public class PropertyExtract
    {
        public string? Value { get; set; }
        public Dictionary<string, string>? Links { get; set; } 
        public string? Name { get; set; }
        public string? Key { get; set; }
    }
}
