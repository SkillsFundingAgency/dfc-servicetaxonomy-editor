using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.ValidateAndRepair
{
    public class ValidateAndRepairContext
    {
        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
        public IContentManager ContentManager { get; set; }
        public INodeWithOutgoingRelationships NodeWithOutgoingRelationships { get; set; }
        public IGraphSyncHelper GraphSyncHelper { get; set; }
        public IGraphValidationHelper GraphValidationHelper { get; set; }
        public IDictionary<string, int> ExpectedRelationshipCounts { get; set; }
        public IValidateAndRepairGraph ValidateAndRepairGraph { get; set; }

        public ValidateAndRepairContext(
            ContentTypePartDefinition contentTypePartDefinition,
            IContentManager contentManager,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts,
            IValidateAndRepairGraph validateAndRepairGraph)
        {
            ContentTypePartDefinition = contentTypePartDefinition;
            ContentManager = contentManager;
            NodeWithOutgoingRelationships = nodeWithOutgoingRelationships;
            GraphSyncHelper = graphSyncHelper;
            GraphValidationHelper = graphValidationHelper;
            ExpectedRelationshipCounts = expectedRelationshipCounts;
            ValidateAndRepairGraph = validateAndRepairGraph;
        }
    }
}
