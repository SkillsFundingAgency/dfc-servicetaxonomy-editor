using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.VersionComparison.Models
{
    public class DiffItem
    {
        public string? Name { get; set; }
        public string? BaseItem { get; set; }
        public Dictionary<string, string>? BaseURLs { get; set; }
        public string? CompareItem { get; set; }
        public Dictionary<string, string>? CompareURLs { get; set; }
    }
}
