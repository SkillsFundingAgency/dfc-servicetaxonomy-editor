using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.DataSync.Extensions;
using DFC.ServiceTaxonomy.DataSync.Helpers;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Models;

namespace DFC.ServiceTaxonomy.DataSync.CosmosDb
{
    public class CosmosDbSubDataSyncQuery : IQuery<SubDataSync>
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
        /// Retrieves a subDataSync (a set of nodes and relationships) centered on a source node, defined by parameters.
        /// The size and shape of the subDataSync is defined by supplied relationship filters and max path size.
        /// </summary>
        /// <param name="nodeLabels">The set of labels that defines the source node.
        /// The source node may have other labels, but to match, it must have all of the supplied labels.</param>
        /// <param name="idPropertyName">The property name to use to match the source node.</param>
        /// <param name="idPropertyValue">The value of the supplied property name used to match the source node.</param>
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
        public CosmosDbSubDataSyncQuery(
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

            if (!(IdPropertyValue is string) && string.IsNullOrEmpty((string)IdPropertyValue))
            {
                validationErrors.Add("IdPropertyValue must be provided.");
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

                var contentType = NodeLabels.First(nodeLabel => !nodeLabel.Equals("Resource", StringComparison.InvariantCultureIgnoreCase)).ToLower();
                (_, var id) = DocumentHelper.GetContentTypeAndId((string)IdPropertyValue);
                return new Query(id.ToString(), contentType);
             }
        }

        public SubDataSync ProcessRecord(IRecord record)
        {
            var nodes = ((List<object>)record["nodes"]).Cast<INode>();

            //todo: is sourcenode always first??
            return new SubDataSync(
                nodes,
                ((List<object>)record["relationships"]).Cast<IRelationship>(),
                nodes.First(n => Equals(n.Properties[IdPropertyName], IdPropertyValue)));
        }
    }
}
