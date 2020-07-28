using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using DFC.ServiceTaxonomy.Neo4j.Queries;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Queries
{
    //todo: common base class with NoteWithOutgoinfRelationships?
    public class NodeAndOutRelationshipsAndTheirInRelationshipsQuery : IQuery<INodeAndOutRelationshipsAndTheirInRelationships?>
    {
        private IEnumerable<string> NodeLabels { get; }
        private string IdPropertyName { get; }
        private object IdPropertyValue { get; }

        public NodeAndOutRelationshipsAndTheirInRelationshipsQuery(
            IEnumerable<string> nodeLabels,
            string idPropertyName,
            object idPropertyValue)
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

            //todo: needs to validate id property name and value too

            return validationErrors;
        }

        public Query Query
        {
            get
            {
                this.CheckIsValid();

//                 return new Query(
// @$"match (s:{string.Join(":", NodeLabels)} {{{IdPropertyName}: '{IdPropertyValue}'}})
// optional match (s)-[r]->(d)
// with s, {{relationship: r, destinationNode: d}} as relationshipDetails
// with {{sourceNode: s, outgoingRelationships: collect(relationshipDetails)}} as sourceNodeWithOutgoingRelationships
// return sourceNodeWithOutgoingRelationships");

                return new Query(
                    $@"match (s:{string.Join(":", NodeLabels)} {{{IdPropertyName}: '{IdPropertyValue}'}})
optional match (s)-[r]->(d)
optional match (d)<-[ir]-(irn) where irn.uri <> s.uri
with s, r, d, {{destIncomingRelationships: collect({{destIncomingRelationship:ir,  destIncomingRelSource:irn}})}} as destIncomingRelationships
with s, {{destNode: d, relationship: r, destinationIncomingRelationships:collect(destIncomingRelationships)}} as relationshipDetails
with {{sourceNode: s, outgoingRelationships: collect(relationshipDetails)}} as nodeAndOutRelationshipsAndTheirInRelationships
return nodeAndOutRelationshipsAndTheirInRelationships");
            }
        }

        public INodeAndOutRelationshipsAndTheirInRelationships? ProcessRecord(IRecord record)
        {
            var results = (Dictionary<string, object>)record["nodeAndOutRelationshipsAndTheirInRelationships"];
            if (results == null)
                throw new QueryResultException($"{nameof(NodeAndOutRelationshipsAndTheirInRelationshipsQuery)} results not in expected format.");

            if (!(results["sourceNode"] is INode sourceNode))
                return null;

            // IEnumerable<(IRelationship, INode)> outgoingRelationships =
            //     ((IEnumerable<object>)results["outgoingRelationships"])
            //     .Cast<IDictionary<string, object>>()
            //     .Select(or =>
            //         ((IRelationship)or["relationship"], (INode)or["destNode"]));
            //
            // if (outgoingRelationships.Count() == 1 && outgoingRelationships.First().Item1 == null)
            //     outgoingRelationships = Enumerable.Empty<(IRelationship, INode)>();

            return new NodeAndOutRelationshipsAndTheirInRelationships(sourceNode, null);
        }
    }
}
