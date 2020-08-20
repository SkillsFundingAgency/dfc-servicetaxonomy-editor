using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using DFC.ServiceTaxonomy.Neo4j.Queries;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries
{
    //todo: common base class with NoteWithOutgoingRelationships?
    //todo: move generic queries into neo4j
    public class NodeAndOutRelationshipsAndTheirInRelationshipsQuery : IQuery<INodeAndOutRelationshipsAndTheirInRelationships?>
    {
        private IEnumerable<string> NodeLabels { get; }
        private List<string>? DestinationUris { get; set; }
        private List<string>? Relationships { get; set; }
        private string IdPropertyName { get; }
        private object IdPropertyValue { get; }

        public NodeAndOutRelationshipsAndTheirInRelationshipsQuery(
            IEnumerable<string> nodeLabels,
            string idPropertyName,
            object idPropertyValue,
            List<string>? destinationUris,
            List<string>? relationships)
        {
            NodeLabels = nodeLabels;
            IdPropertyName = idPropertyName;
            IdPropertyValue = idPropertyValue;
            DestinationUris = destinationUris;
            Relationships = relationships;
        }

        public List<string> ValidationErrors()
        {
            var validationErrors = new List<string>();

            if (!NodeLabels.Any())
            {
                validationErrors.Add("At least one NodeLabel must be provided.");
            }

            //todo: needs to validate id property name and value too

            return validationErrors;
        }

        private string GetDestinationNodes(string prefix, string destinationNodeName)
        {
            if (DestinationUris == null || DestinationUris.Count == 0)
            {
                return string.Empty;
            }

            var uris = DestinationUris.ToArray();

            return $" {prefix} {destinationNodeName}.uri IN {JsonSerializer.Serialize(uris)}";
        }

        public Query Query
        {
            get
            {
                this.CheckIsValid();

                // irn might have a different IdPropertyName, but if it has, it isn't the source node, so the where is ok
                return new Query(
                    $@"match (s:{string.Join(":", NodeLabels)} {{{IdPropertyName}: '{IdPropertyValue}'}})
optional match (s)-[{GetRelationships("r")}]->(d) {GetDestinationNodes("WHERE", "d")}
optional match (d)<-[{GetRelationships("ir")}]-(irn) where irn.{IdPropertyName} <> s.{IdPropertyName} {GetDestinationNodes("AND", "irn")}
with s, {{destNode: d, relationship: r, destinationIncomingRelationships:collect({{destIncomingRelationship:ir,  destIncomingRelSource:irn}})}} as relationshipDetails
with {{sourceNode: s, outgoingRelationships: collect(relationshipDetails)}} as nodeAndOutRelationshipsAndTheirInRelationships
return nodeAndOutRelationshipsAndTheirInRelationships");
            }
        }

        private string GetRelationships(string v)
        {
            if(Relationships == null || Relationships.Count == 0)
            {
                return v;
            }

            return $"{v}:{string.Join('|', Relationships)}";
        }

        public INodeAndOutRelationshipsAndTheirInRelationships? ProcessRecord(IRecord record)
        {
            var results = (Dictionary<string, object>)record["nodeAndOutRelationshipsAndTheirInRelationships"];
            if (results == null)
                throw new QueryResultException($"{nameof(NodeAndOutRelationshipsAndTheirInRelationshipsQuery)} results not in expected format.");

            if (!(results["sourceNode"] is INode sourceNode))
                return null;

            //todo: return as IOutgoingRelationships, rather than tuple then convert

            IEnumerable<(IRelationship, INode, IEnumerable<(IRelationship, INode)>)> outgoingRelationships =
                ((IEnumerable<object>)results["outgoingRelationships"])
                .Cast<IDictionary<string, object>>()
                .Select(or =>
                    ((IRelationship)or["relationship"],
                        (INode)or["destNode"],
                        ((IEnumerable<object>)or["destinationIncomingRelationships"])
                        .Cast<IDictionary<string, object>>()
                        .Select(ir =>
                            ((IRelationship)ir["destIncomingRelationship"],
                                (INode)ir["destIncomingRelSource"]))
                        .Where(t => t.Item1 != null)))
                .Where(t => t.Item1 != null);

            return new NodeAndOutRelationshipsAndTheirInRelationships(sourceNode, outgoingRelationships);
        }
    }
}

