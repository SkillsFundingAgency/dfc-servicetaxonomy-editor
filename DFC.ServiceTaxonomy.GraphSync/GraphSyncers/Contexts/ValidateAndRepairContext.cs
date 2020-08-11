using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class ValidateAndRepairContext : GraphOperationContext, IValidateAndRepairContext
    {
        public INodeWithOutgoingRelationships NodeWithOutgoingRelationships { get; }
        public IGraphValidationHelper GraphValidationHelper { get; }
        public IDictionary<string, int> ExpectedRelationshipCounts { get; }
        public IValidateAndRepairGraph ValidateAndRepairGraph { get; }

        public ValidateAndRepairContext(
            ContentItem contentItem,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IValidateAndRepairGraph validateAndRepairGraph)
            : base(contentItem, graphSyncHelper, contentManager, contentItemVersion, null)
        {
            ContentItemVersion = contentItemVersion;
            NodeWithOutgoingRelationships = nodeWithOutgoingRelationships;
            GraphValidationHelper = graphValidationHelper;
            ValidateAndRepairGraph = validateAndRepairGraph;

            ExpectedRelationshipCounts = new Dictionary<string, int>();
        }

    }
}
