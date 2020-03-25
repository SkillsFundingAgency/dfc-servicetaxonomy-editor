using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Queries
{
    public class MatchNodeWithAllOutgoingRelationshipsQuery : IQuery<IRecord>
    {
        private IEnumerable<string> NodeLabels { get; }
        private string IdPropertyName { get; }
        private object IdPropertyValue { get; }

        public MatchNodeWithAllOutgoingRelationshipsQuery(IEnumerable<string> nodeLabels, string idPropertyName, object idPropertyValue)
        {
            NodeLabels = nodeLabels;
            IdPropertyName = idPropertyName;
            IdPropertyValue = idPropertyValue;
        }

        public List<string> ValidationErrors()
        {
            return new List<string>();
        }

        public Query Query
        {
            get
            {
                return new Query($"match (s:{string.Join(":", NodeLabels)} {{{IdPropertyName}: '{IdPropertyValue}'}}) optional match (s)-[r]->(d) return s, r, d");
            }
        }

        public IRecord ProcessRecord(IRecord record)
        {
            return record;
        }
    }
}
