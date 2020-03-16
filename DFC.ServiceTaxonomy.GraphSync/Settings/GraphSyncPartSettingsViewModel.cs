using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DFC.ServiceTaxonomy.GraphSync.Settings
{
    public class GraphSyncPartSettingsViewModel
    {
        public string? NamespacePrefix { get; set; }
        public string? BagPartContentItemRelationshipType { get; set; }
        public bool PreexistingNode { get; set; }
        public string? NodeNameTransform { get; set; }
        public string? PropertyNameTransform { get; set; }
        public string? CreateRelationshipType { get; set; }    //or RelationshipTypeTransform for consistency?
        public string? IdPropertyName { get; set; }
        public string? IdPropertyValueTransform { get; set; }

        [BindNever]
        public List<string>? NamespacePrefixOptions { get; set; }
    }
}
