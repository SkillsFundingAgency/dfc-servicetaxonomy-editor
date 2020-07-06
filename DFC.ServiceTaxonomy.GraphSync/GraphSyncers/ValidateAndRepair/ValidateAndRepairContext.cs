using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Wrappers;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.ValidateAndRepair
{
    //todo: contexts folder, and move top level back to top level
    //todo: interfaces for contexts
    public class ValidateAndRepairContext
    {
        public INodeWithOutgoingRelationships NodeWithOutgoingRelationships { get; }
        public IGraphSyncHelper GraphSyncHelper { get; }
        public IGraphValidationHelper GraphValidationHelper { get; }
        public IDictionary<string, int> ExpectedRelationshipCounts { get; }
        public IValidateAndRepairGraph ValidateAndRepairGraph { get; }

        public IContentManager ContentManager { get; }
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public IContentPartFieldDefinition? ContentPartFieldDefinition  { get; private set; }

        public ValidateAndRepairContext(
            IContentManager contentManager,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts,
            IValidateAndRepairGraph validateAndRepairGraph)
        {
            ContentTypePartDefinition = default!;
            ContentManager = contentManager;
            NodeWithOutgoingRelationships = nodeWithOutgoingRelationships;
            GraphSyncHelper = graphSyncHelper;
            GraphValidationHelper = graphValidationHelper;
            ExpectedRelationshipCounts = expectedRelationshipCounts;
            ValidateAndRepairGraph = validateAndRepairGraph;
        }

        public void SetContentPartFieldDefinition(ContentPartFieldDefinition? contentPartFieldDefinition)
        {
            ContentPartFieldDefinition = contentPartFieldDefinition != null
                ? new ContentPartFieldDefinitionWrapper(contentPartFieldDefinition) : default;
        }
    }
}
