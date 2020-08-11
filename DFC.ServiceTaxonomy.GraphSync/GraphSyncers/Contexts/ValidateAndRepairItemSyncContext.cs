using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
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
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IValidateAndRepairGraph validateAndRepairGraph,
            ContentTypeDefinition contentTypeDefinition,
            object nodeId)

            : base(contentItem, contentManager, contentItemVersion, nodeWithOutgoingRelationships,
                graphSyncHelper, graphValidationHelper, validateAndRepairGraph)
        {
            ContentTypeDefinition = contentTypeDefinition;
            NodeId = nodeId;
        }
    }
}
