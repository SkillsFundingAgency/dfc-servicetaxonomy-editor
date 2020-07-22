using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Wrappers;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class ValidateAndRepairContext : IValidateAndRepairContext
    {
        public INodeWithOutgoingRelationships NodeWithOutgoingRelationships { get; }
        public IGraphSyncHelper GraphSyncHelper { get; }
        public IGraphValidationHelper GraphValidationHelper { get; }
        public IDictionary<string, int> ExpectedRelationshipCounts { get; }
        public IValidateAndRepairGraph ValidateAndRepairGraph { get; }

        public IContentManager ContentManager { get; }
        public IContentItemVersion ContentItemVersion { get; }
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public IContentPartFieldDefinition? ContentPartFieldDefinition  { get; private set; }

        public ValidateAndRepairContext(IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IValidateAndRepairGraph validateAndRepairGraph)
        {
            ContentManager = contentManager;
            ContentItemVersion = contentItemVersion;
            NodeWithOutgoingRelationships = nodeWithOutgoingRelationships;
            GraphSyncHelper = graphSyncHelper;
            GraphValidationHelper = graphValidationHelper;
            ValidateAndRepairGraph = validateAndRepairGraph;

            ExpectedRelationshipCounts = new Dictionary<string, int>();

            ContentTypePartDefinition = default!;
        }

        public void SetContentPartFieldDefinition(ContentPartFieldDefinition? contentPartFieldDefinition)
        {
            ContentPartFieldDefinition = contentPartFieldDefinition != null
                ? new ContentPartFieldDefinitionWrapper(contentPartFieldDefinition) : default;
        }
    }
}
