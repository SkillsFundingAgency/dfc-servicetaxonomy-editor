using System.Collections.Generic;
using System.Linq;
using System.Text;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries
{
    public class NodeAndIncomingRelationshipsQuery : IQuery<INodeAndOutRelationshipsAndTheirInRelationships?>
    {
        private readonly IEnumerable<string> _sourceNodeLabels;
        private readonly string _sourceNodePropertyIdName;
        private readonly string _sourceNodeId;

        public NodeAndIncomingRelationshipsQuery(
            IEnumerable<string> sourceNodeLabels, string sourceNodePropertyIdName, string sourceNodeId)
        {
            _sourceNodeLabels = sourceNodeLabels;
            _sourceNodePropertyIdName = sourceNodePropertyIdName;
            _sourceNodeId = sourceNodeId;
        }

        public List<string> ValidationErrors()
        {
            return new List<string>();
        }

        public Query Query
        {
            get
            {
                var commandStringBuilder = new StringBuilder($"match (s)-[r]->(d:{string.Join(":", _sourceNodeLabels)} {{{_sourceNodePropertyIdName}: '{_sourceNodeId}'}})");
                commandStringBuilder.AppendLine(" with s, {destNode: d, relationship: r, destinationIncomingRelationships:collect({destIncomingRelationship:'todo',  destIncomingRelSource:'todo'})} as relationshipDetails");
                commandStringBuilder.AppendLine(" with { sourceNode: s, outgoingRelationships: collect(relationshipDetails)} as nodeAndOutRelationshipsAndTheirInRelationships");
                commandStringBuilder.AppendLine(" return nodeAndOutRelationshipsAndTheirInRelationships");
                return new Query(commandStringBuilder.ToString());
            }
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
