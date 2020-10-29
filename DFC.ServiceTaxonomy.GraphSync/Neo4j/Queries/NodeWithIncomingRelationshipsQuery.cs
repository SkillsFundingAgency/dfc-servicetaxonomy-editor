using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using DFC.ServiceTaxonomy.Neo4j.Queries;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries
{
    //todo: INodeWithOutgoingRelationships => INodeWithRelationships, use for out & in
    // derived to supply < or >, or have a flag for incoming or outgoing
    // think separate, but with shared base
    public class NodeWithIncomingRelationshipsQuery : IQuery<INodeWithIncomingRelationships?>
    {
        private int MaxPathLength { get; }
        private IEnumerable<string> NodeLabels { get; }
        private string IdPropertyName { get; }
        private object IdPropertyValue { get; }

        public NodeWithIncomingRelationshipsQuery(
            IEnumerable<string> nodeLabels,
            string idPropertyName,
            object idPropertyValue,
            int maxPathLength = 1)
        {
            MaxPathLength = maxPathLength;
            NodeLabels = nodeLabels;
            IdPropertyName = idPropertyName;
            IdPropertyValue = idPropertyValue;
        }

        public List<string> ValidationErrors()
        {
            var validationErrors = new List<string>();

            if (!NodeLabels.Any())
            {
                validationErrors.Add("At least one NodeLabel must be provided.");
            }

            if (MaxPathLength < 1)
            {
                validationErrors.Add("MaxPathLength must be positive.");
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
optional match (s)<-[r*1..{MaxPathLength}]-(d)
with s, {{relationship: r, destinationNode: d}} as relationshipDetails
with {{sourceNode: s, incomingRelationships: collect(relationshipDetails)}} as sourceNodeWithIncomingRelationships
return sourceNodeWithIncomingRelationships");
            }
        }

        //todo: need to update to handle multi path
        public INodeWithIncomingRelationships? ProcessRecord(IRecord record)
        {
            var results = (Dictionary<string, object>)record["sourceNodeWithIncomingRelationships"];
            if (results == null)
                throw new QueryResultException($"{nameof(NodeWithIncomingRelationshipsQuery)} results not in expected format.");

            if (!(results["sourceNode"] is INode sourceNode))
                return null;

            IEnumerable<(IRelationship, INode)> incomingRelationships =
                ((IEnumerable<object>)results["incomingRelationships"])
                .Cast<IDictionary<string, object>>()
                .Select(or =>
                    ((IRelationship)or["relationship"], (INode)or["destinationNode"]));

            if (incomingRelationships.Count() == 1 && incomingRelationships.First().Item1 == null)
                incomingRelationships = Enumerable.Empty<(IRelationship, INode)>();

            INodeWithIncomingRelationships nodeWithIncoming =
                //todo: check all combos of missing data
                new NodeWithIncomingRelationships(sourceNode, incomingRelationships);

            return nodeWithIncoming;
        }
    }
}
