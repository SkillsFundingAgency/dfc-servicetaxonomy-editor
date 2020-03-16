﻿
namespace DFC.ServiceTaxonomy.GraphSync.Settings
{
    public class GraphSyncPartSettings
    {
        public string? NamespacePrefix { get; set; }
        //todo: probably better to just have RelationshipType and use that for both bag and non-bag and don't lift from hint
        public string? BagPartContentItemRelationshipType { get; set; }
        public bool PreexistingNode { get; set; }
        //public string NodePrefix { get; set; }
        public string? NodeNameTransform { get; set; }
        public string? PropertyNameTransform { get; set; }
        public string? IdPropertyName { get; set; }
        public string? IdPropertyValueTransform { get; set; }
    }
}
