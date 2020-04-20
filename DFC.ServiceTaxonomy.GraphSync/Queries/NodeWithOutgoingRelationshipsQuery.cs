using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Queries;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Queries
{
    public class NodeWithOutgoingRelationshipsQuery : IQuery<IRecord>
    {
        private IEnumerable<string> NodeLabels { get; }
        private string IdPropertyName { get; }
        private object IdPropertyValue { get; }

        public NodeWithOutgoingRelationshipsQuery(IEnumerable<string> nodeLabels, string idPropertyName, object idPropertyValue)
        {
            NodeLabels = nodeLabels;
            IdPropertyName = idPropertyName;
            IdPropertyValue = idPropertyValue;
        }

        public List<string> ValidationErrors()
        {
            var validationErrors = new List<string>();

            if(!NodeLabels.Any())
            {
                validationErrors.Add("At least one NodeLabel must be provided.");
            }

            return validationErrors;
        }

        public Query Query
        {
            get
            {
                this.CheckIsValid();

                return new Query(
@$"match (s:{string.Join(":", NodeLabels)} {{{IdPropertyName}: '{IdPropertyValue}'}})
optional match (s)-[r]->(d)
with s, {{relationship: r, destinationNode: d}} as relationshipDetails
with {{sourceNode: o, outgoingRelationships: collect(relationshipDetails)}} as sourceNodeWithOutgoingRelationships
return sourceNodeWithOutgoingRelationships");
            }
        }

        public IRecord ProcessRecord(IRecord record)
        {
            return record;
        }
    }
}
