using System;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class ValidateAndRepairItemSyncContext : ValidateAndRepairContext, IValidateAndRepairItemSyncContext
    {
        public ContentTypeDefinition ContentTypeDefinition { get; }
        public object NodeId { get; }

        public ValidateAndRepairItemSyncContext(
            ContentItem contentItem,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            ISubgraph nodeWithIncomingRelationships,
            ISyncNameProvider syncNameProvider,
            IGraphValidationHelper graphValidationHelper,
            IValidateAndRepairGraph validateAndRepairGraph,
            ContentTypeDefinition contentTypeDefinition,
            object nodeId,
            IServiceProvider serviceProvider)

            : base(contentItem, contentManager, contentItemVersion, nodeWithOutgoingRelationships,
                nodeWithIncomingRelationships, syncNameProvider, graphValidationHelper, validateAndRepairGraph,
                serviceProvider.GetRequiredService<ILogger<ValidateAndRepairItemSyncContext>>())
        {
            ContentTypeDefinition = contentTypeDefinition;
            NodeId = nodeId;
        }
    }
}
