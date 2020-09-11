using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries
{
    //todo: common base class with NoteWithOutgoingRelationships?
    //todo: move generic queries into neo4j
    public class NodeAndNestedOutgoingRelationshipsQuery : IQuery<INodeAndOutRelationshipsAndTheirInRelationships?>
    {
        private readonly string? _command;
        private readonly IEnumerable<string>? _sourceNodeLabels;
        private readonly string? _sourceNodePropertyIdName;
        private readonly string? _sourceNodeId;
        private readonly bool _ignoreIncomingRelationship;

        public NodeAndNestedOutgoingRelationshipsQuery(
            string? command, IEnumerable<string>? sourceNodeLabels = null, string? sourceNodePropertyIdName = null, string? sourceNodeId = null, bool ignoreOutgoingRelationship = false)
        {
            _command = command;
            _sourceNodeLabels = sourceNodeLabels;
            _sourceNodePropertyIdName = sourceNodePropertyIdName;
            _sourceNodeId = sourceNodeId;
            _ignoreIncomingRelationship = ignoreOutgoingRelationship;
        }

        public List<string> ValidationErrors()
        {
            return new List<string>();
        }

        public Query Query
        {
            get
            {
                if (!_ignoreIncomingRelationship)
                {
                    return BuildCommand();
                }
                else
                {
                    return BuildSourceNodeCommand();
                }
            }
        }

        private Query BuildSourceNodeCommand()
        {
            var commandStringBuilder = new StringBuilder($"match (s:{string.Join(":", _sourceNodeLabels!)} {{{_sourceNodePropertyIdName}: '{_sourceNodeId}'}})");
            commandStringBuilder.AppendLine(" with { sourceNode: s, outgoingRelationships: collect(null) } as nodeAndOutRelationshipsAndTheirInRelationships");
            commandStringBuilder.AppendLine(" return nodeAndOutRelationshipsAndTheirInRelationships");
            return new Query(commandStringBuilder.ToString());
        }

        private Query BuildCommand()
        {
            var withStringBuilder = new StringBuilder();

            int currentDepth = 1;
            int depthCount = Regex.Matches(_command, "d[0-9]+:").Count;

            List<string> collectClauses = new List<string>();
            List<string> relationshipClauses = new List<string>();


            while (currentDepth <= depthCount)
            {
                relationshipClauses.Add($"{{destNode: d{currentDepth}, relationship: r{currentDepth}, destinationIncomingRelationships:collect({{destIncomingRelationship:'todo',  destIncomingRelSource:'todo'}})}} as dr{currentDepth}RelationshipDetails");

                collectClauses.Add($"collect(dr{currentDepth}RelationshipDetails)");
                currentDepth++;
            }

            withStringBuilder.AppendLine($"with s,{string.Join(',', relationshipClauses)}");
            withStringBuilder.AppendLine($"with {{sourceNode: s, outgoingRelationships: {string.Join('+', collectClauses)}}} as nodeAndOutRelationshipsAndTheirInRelationships");
            withStringBuilder.AppendLine("return nodeAndOutRelationshipsAndTheirInRelationships");

            return new Query($"{_command} {withStringBuilder}");
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
