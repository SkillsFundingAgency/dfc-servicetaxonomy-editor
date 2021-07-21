using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DFC.ServiceTaxonomy.VersionCompare.ViewModels
{
    public class VersionComparisonViewModel
    {
        public string? BaseVersionId { get; set; }
        public int BaseVersionNumber { get; set; }
        public string? BaseVersionJson { get; set; } 
        public string? CompareVersionJson { get; set; }
        public string? CompareVersionId { get; set; }
        public int CompareVersionNumber { get; set; }
        public List<SelectListItem>? Options { get; set; }
        public string? SelectedProperty { get; set; }
        public List<SelectListItem>? Properties { get; set; }
        public List<PropertyDiffViewModel>? PropertyDiffs { get; set; }
    }
}
