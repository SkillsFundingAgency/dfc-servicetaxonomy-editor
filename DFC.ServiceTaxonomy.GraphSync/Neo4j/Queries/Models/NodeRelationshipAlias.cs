using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Models
{
    public class NodeRelationshipAlias
    {
        public string? RelationshipAlias { get; set; }
        public List<string>? SourceLabels { get; set; }
        public string? SourceAlias { get; set; }
        public string? Relationship { get; set; }
        public string? DestinationAlias { get; set; }
        public List<string>? DestinationLabels { get; set; }
    }
}
