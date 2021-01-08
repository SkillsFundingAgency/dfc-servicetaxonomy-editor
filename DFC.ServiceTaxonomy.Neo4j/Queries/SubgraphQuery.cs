using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Model;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Queries
{
    //todo: replace usage of NodeWithIncomingRelationshipsQuery with this more general case
    public class SubgraphQuery : IQuery<ISubgraph>
    {
        public const string? RelationshipFilterNone = null;
        public const string RelationshipFilterIncoming = "<";
        public const string RelationshipFilterOutgoing = ">";

        private IEnumerable<string> NodeLabels { get; }
        private string IdPropertyName { get; }
        private object IdPropertyValue { get; }
        private string? RelationshipFilter { get; }
        private int MaxPathLength { get; }

        //todo: support separate maxPathLength for incoming and outgoing?
        // options:
        // post filtering? - could end up doing way too much work
        // 2 calls? - not atomic, unless in single transaction
        // leave to consumer??

        /// <summary>
        /// See https://neo4j.com/labs/apoc/4.1/graph-querying/expand-subgraph/ for underlying cypher.
        /// </summary>
        /// <param name="relationshipFilter">Syntax: [&lt;]RELATIONSHIP_TYPE1[&gt;]|[&lt;]RELATIONSHIP_TYPE2[&gt;]|...
        /// <list type="table">
        /// <listheader>
        /// <term>Input</term>
        /// <term>Type</term>
        /// <term>Direction</term>
        /// <item><term>null</term><term>any type</term><term>BOTH</term></item>
        /// <item><term>LIKES&gt;</term><term>LIKES</term><term>OUTGOING</term></item>
        /// <item><term>&lt;FOLLOWS</term><term>FOLLOWS</term><term>INCOMING</term></item>
        /// <item><term>KNOWS</term><term>KNOWS</term><term>BOTH</term></item>
        /// <item><term>&gt;</term><term>any type</term><term>OUTGOING</term></item>
        /// <item><term>&lt;</term><term>any type</term><term>INCOMING</term></item>
        /// </listheader>
        /// </list>
        /// </param>
        /// <param name="maxPathLength">The maximum number of hops in the traversal.</param>
        public SubgraphQuery(
            IEnumerable<string> nodeLabels,
            string idPropertyName,
            object idPropertyValue,
            string? relationshipFilter = RelationshipFilterNone,
            int maxPathLength = 1)
        {
            RelationshipFilter = relationshipFilter;
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

            if (MaxPathLength < 0)
            {
                validationErrors.Add("MaxPathLength must not be negative.");
            }

            return validationErrors;
        }

        public Query Query
        {
            get
            {
                this.CheckIsValid();

                var parameters = new Dictionary<string, object?>
                {
                    {"maxLevel", MaxPathLength},
                    {"idPropertyValue", IdPropertyValue},
                    {"relationshipFilter", RelationshipFilter}
                };

                return new Query(
                    @$"match (n:{string.Join(":", NodeLabels)} {{{IdPropertyName}:$idPropertyValue}})
call apoc.path.subgraphAll(n, {{maxLevel: $maxLevel, relationshipFilter: $relationshipFilter}}) yield nodes, relationships
return nodes, relationships", parameters);
            }
        }

        public ISubgraph ProcessRecord(IRecord record)
        {
            var nodes = ((List<object>)record["nodes"]).Cast<INode>();

            //todo: is sourcenode always first??
            return new Subgraph(
                nodes,
                ((List<object>)record["relationships"]).Cast<IRelationship>(),
                nodes.First(n => Equals(n.Properties[IdPropertyName], IdPropertyValue)));
        }
    }
}
