using System.Collections.Generic;
using OrchardCore.DisplayManagement;

namespace DFC.ServiceTaxonomy.VersionComparison.ViewModels
{
    public class VersionComparisonViewModel
    {
        public string? ContentItemId { get; set; }
        public string? ContentItemDisplayName { get; set; }
        public string? ContentItemContentType { get; set; }
        public dynamic? SelectLists { get; set; }
        public List<IShape>? DiffItems { get; set; }
    }
}
