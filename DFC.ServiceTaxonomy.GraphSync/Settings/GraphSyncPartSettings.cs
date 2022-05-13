﻿
namespace DFC.ServiceTaxonomy.GraphSync.Settings
{
    public class GraphSyncPartSettings
    {
        //todo: probably better to just have RelationshipType and use that for both bag and non-bag and don't lift from hint
        public string? Name { get; set; }
        public string? BagPartContentItemRelationshipType { get; set; }
        public string? PreExistingNodeUriPrefix { get; set; }
        public bool PreexistingNode { get; set; }
        public string? NodeNameTransform { get; set; }
        public string? PropertyNameTransform { get; set; }
        public string? CreateRelationshipType { get; set; }    //or RelationshipTypeTransform for consistency? //todo: is this actually used?
        public string? IdPropertyName { get; set; }
        public string? GenerateIdPropertyValue { get; set; }
        public bool DisplayId { get; set; }
        //todo: rename
        public int? VisualiserNodeDepth { get; set; }
        public int? VisualiserIncomingRelationshipsPathLength { get; set; }
    }
}
