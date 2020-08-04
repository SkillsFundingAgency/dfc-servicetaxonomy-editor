using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Commands;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using DFC.ServiceTaxonomy.Neo4j.Queries;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries
{
    //todo: common base with NodeWithOutgoingRelationshipsQuery??
    //rename ghost?
    public class GetDraftRelationships : IGetDraftRelationships
    {
        //private IEnumerable<string> NodeLabels { get; }
        public string? ContentType { get; set; }
        public object? IdPropertyValue { get; set; }

        // we'll also need to filter by incoming source id & relationship type
        // but probably better to fetch all, then filter later in code?
        // public GetDraftRelationships(IEnumerable<string> nodeLabels, string idPropertyName, object idPropertyValue)
        // {
        //     NodeLabels = nodeLabels;
        // public GetDraftRelationships(string contentType, object idPropertyValue)
        // {
        //     ContentType = contentType;
        //     IdPropertyValue = idPropertyValue;
        // }

        public List<string> ValidationErrors()
        {
            var validationErrors = new List<string>();

            if (ContentType == null)
                validationErrors.Add($"{nameof(ContentType)} is null.");

            if (IdPropertyValue == null)
                validationErrors.Add($"{nameof(IdPropertyValue)} is null.");

            return validationErrors;
        }

        public Query Query
        {
            get
            {
                this.CheckIsValid();

                //todo: non-string id's wouldn't need ''
                //todo: could simplify as will only ever be a single triplet ()-[]->()
                return new Query(
                    @$"match (s)-[r]->(d:{UpdateDraftRelationships.GhostLabelPrefix}{ContentType} {{{UpdateDraftRelationships.PreviewIdPropertyName}: `{IdPropertyValue}`}})
with s, {{relationship: r, destinationNode: d}} as relationshipDetails
with {{sourceNode: s, outgoingRelationships: collect(relationshipDetails)}} as sourceNodeWithOutgoingRelationships
return sourceNodeWithOutgoingRelationships");
// @$"match (s:{string.Join(":", NodeLabels)} {{{IdPropertyName}: '{IdPropertyValue}'}})
// optional match (s)-[r]->(d)
// with s, {{relationship: r, destinationNode: d}} as relationshipDetails
// with {{sourceNode: s, outgoingRelationships: collect(relationshipDetails)}} as sourceNodeWithOutgoingRelationships
// return sourceNodeWithOutgoingRelationships");
            }
        }

        public INodeWithOutgoingRelationships? ProcessRecord(IRecord record)
        {
            var results = (Dictionary<string, object>)record["sourceNodeWithOutgoingRelationships"];
            if (results == null)
                throw new QueryResultException($"{nameof(NodeWithOutgoingRelationshipsQuery)} results not in expected format.");

            if (!(results["sourceNode"] is INode sourceNode))
                return null;

            IEnumerable<(IRelationship, INode)> outgoingRelationships =
                ((IEnumerable<object>)results["outgoingRelationships"])
                .Cast<IDictionary<string, object>>()
                .Select(or =>
                    ((IRelationship)or["relationship"], (INode)or["destinationNode"]));

            if (outgoingRelationships.Count() == 1 && outgoingRelationships.First().Item1 == null)
                outgoingRelationships = Enumerable.Empty<(IRelationship, INode)>();

            NodeWithOutgoingRelationships nodeWithOutgoingRelationships =
                //todo: check all combos of missing data
                new NodeWithOutgoingRelationships(sourceNode, outgoingRelationships);

            return nodeWithOutgoingRelationships;
        }
    }
}
