using DFC.ServiceTaxonomy.Editor.Module.Settings;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DFC.ServiceTaxonomy.Editor.Module.ViewModels
{
    public class GraphLookupPartSettingsViewModel
    {
        public string? DisplayName { get; set; }
        public string? Description { get; set; }

        public string? NodeLabel { get; set; }
        //public List<string>? DisplayFieldName { get; set; }
        public string? DisplayFieldName { get; set; }
        public string? ValueFieldName { get; set; }
        public string? RelationshipType { get; set; }
        public bool NodesAreReadonly { get; set; }

        [BindNever]
        public GraphLookupPartSettings? GraphLookupPartSettings { get; set; }
    }
}
