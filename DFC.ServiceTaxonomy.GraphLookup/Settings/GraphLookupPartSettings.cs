﻿namespace DFC.ServiceTaxonomy.GraphLookup.Settings
{
    public class GraphLookupPartSettings
    {
        public string? DisplayName { get; set; }
        public string? Hint { get; set; }

        public bool Multiple { get; set; }

        public string? NodeLabel { get; set; }
        public string? DisplayFieldName { get; set; }
        public string? ValueFieldName { get; set; }
        public string? RelationshipType { get; set; }
        public string? PropertyName { get; set; }
        public bool NodesAreReadonly { get; set; }
    }
}
