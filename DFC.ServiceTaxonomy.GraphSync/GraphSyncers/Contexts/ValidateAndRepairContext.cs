using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class ValidateAndRepairContext : GraphOperationContext, IValidateAndRepairContext
    {
        //todo: there can (should) be only one
        public INodeWithOutgoingRelationships NodeWithOutgoingRelationships { get; }
        public INodeWithIncomingRelationships NodeWithIncomingRelationships { get; }
        public IGraphValidationHelper GraphValidationHelper { get; }
        public IDictionary<string, int> ExpectedRelationshipCounts { get; }
        public IValidateAndRepairGraph ValidateAndRepairGraph { get; }

        public ValidateAndRepairContext(
            ContentItem contentItem,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            INodeWithIncomingRelationships nodeWithIncomingRelationships,
            ISyncNameProvider syncNameProvider,
            IGraphValidationHelper graphValidationHelper,
            IValidateAndRepairGraph validateAndRepairGraph,
            ILogger logger)
            : base(contentItem, syncNameProvider, contentManager, contentItemVersion, logger)
        {
            ContentItemVersion = contentItemVersion;
            NodeWithOutgoingRelationships = nodeWithOutgoingRelationships;
            NodeWithIncomingRelationships = nodeWithIncomingRelationships;
            GraphValidationHelper = graphValidationHelper;
            ValidateAndRepairGraph = validateAndRepairGraph;

            ExpectedRelationshipCounts = new Dictionary<string, int>();
        }

    }
}
