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
    //todo: this might be used to get incoming relationships, but it looks like it actually gets outgoing relationships
    // so we should be able to get rid of this and use one of the other queries
    public class NodeAndIncomingRelationshipsQuery : IQuery<INodeAndOutRelationshipsAndTheirInRelationships?>
    {
        private readonly IEnumerable<string> _sourceNodeLabels;
        private readonly string _sourceNodePropertyIdName;
        private readonly string _sourceNodeId;
        private readonly bool _ignoreIncomingRelationship;

        public NodeAndIncomingRelationshipsQuery(
            IEnumerable<string> sourceNodeLabels, string sourceNodePropertyIdName, string sourceNodeId, bool ignoreIncomingRelationship = false)
        {
            _sourceNodeLabels = sourceNodeLabels;
            _sourceNodePropertyIdName = sourceNodePropertyIdName;
            _sourceNodeId = sourceNodeId;
            _ignoreIncomingRelationship = ignoreIncomingRelationship;
        }

        public List<string> ValidationErrors()
        {
            var validationErrors = new List<string>();

            if (!_sourceNodeLabels.Any())
            {
                validationErrors.Add("At least one NodeLabel must be provided.");
            }

            return validationErrors;
        }

        public Query Query
        {
            get
            {
                if (!_ignoreIncomingRelationship)
                {
                    var commandStringBuilder = new StringBuilder($"match (s)-[r]->(d:{string.Join(":", _sourceNodeLabels)} {{{_sourceNodePropertyIdName}: '{_sourceNodeId}'}})");
                    commandStringBuilder.AppendLine(" with s, {destNode: d, relationship: r, destinationIncomingRelationships:collect({destIncomingRelationship:'',  destIncomingRelSource:'todo'})} as relationshipDetails");
                    commandStringBuilder.AppendLine(" with { sourceNode: s, outgoingRelationships: collect(relationshipDetails)} as nodeAndOutRelationshipsAndTheirInRelationships");
                    commandStringBuilder.AppendLine(" return nodeAndOutRelationshipsAndTheirInRelationships");
                    return new Query(commandStringBuilder.ToString());
                }
                else
                {
                    var commandStringBuilder = new StringBuilder($"match (s:{string.Join(":", _sourceNodeLabels)} {{{_sourceNodePropertyIdName}: '{_sourceNodeId}'}})");
                    commandStringBuilder.AppendLine(" with { sourceNode: s, outgoingRelationships: collect(null) } as nodeAndOutRelationshipsAndTheirInRelationships");
                    commandStringBuilder.AppendLine(" return nodeAndOutRelationshipsAndTheirInRelationships");
                    return new Query(commandStringBuilder.ToString());
                }
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
