using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public class NodeAndNestedOutgoingRelationshipsQuery : IQuery<INodeAndOutRelationshipsAndTheirInRelationships?>
    {
        private readonly string _command;

        public NodeAndNestedOutgoingRelationshipsQuery(string command)
        {
            _command = command;
        }

        public List<string> ValidationErrors()
        {
            return new List<string>();
        }

        public Query Query
        {
            get
            {
                this.CheckIsValid();

                return BuildCommand();
            }
        }

        private Query BuildCommand()
        {
            var withStringBuilder = new StringBuilder();

            int currentDepth = 0;
            //todo: brittle
            int depthCount = (Regex.Matches(_command, "d[0-9]+:").Count)-1;

            List<string> collectClauses = new List<string>();
            List<string> relationshipClauses = new List<string>();

            while (currentDepth <= depthCount)
            {
                //todo: destinationIncomingRelationships
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
