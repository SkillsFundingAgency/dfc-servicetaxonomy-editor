using DFC.ServiceTaxonomy.Editor.Module.Parts;
using DFC.ServiceTaxonomy.Editor.Module.Settings;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.Editor.Module.ViewModels
{
    public class GraphLookupPartViewModel
    {
        public string? Value { get; set; }
        public string? DisplayText { get; set; }

        [BindNever]
        public ContentItem? ContentItem { get; set; }

        [BindNever]
        public GraphLookupPart? GraphLookupPart { get; set; }

        [BindNever]
        public GraphLookupPartSettings? Settings { get; set; }
    }
}
