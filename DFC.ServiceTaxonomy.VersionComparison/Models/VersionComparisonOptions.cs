
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DFC.ServiceTaxonomy.VersionComparison.Models
{
    public class VersionComparisonOptions
    {
        public string? ContentItemId { get; set; }
        public string? BaseVersion { get; set; }
        public string? CompareVersion { get; set; }

        [BindNever]
        public List<SelectListItem>? BaseVersionSelectListItems { get; set; }
        [BindNever]
        public List<SelectListItem>? CompareVersionSelectListItems { get; set; }
    }
}
