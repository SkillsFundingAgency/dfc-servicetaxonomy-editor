using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IValidateAndRepairContext
    {
        public IContentManager ContentManager { get; }
        public ContentTypePartDefinition ContentTypePartDefinition { get; }
        public IContentPartFieldDefinition? ContentPartFieldDefinition  { get; }

        public INodeWithOutgoingRelationships NodeWithOutgoingRelationships { get; }
        public IGraphSyncHelper GraphSyncHelper { get; }
        public IGraphValidationHelper GraphValidationHelper { get; }
        public IDictionary<string, int> ExpectedRelationshipCounts { get; }
        public IValidateAndRepairGraph ValidateAndRepairGraph { get; }

        void SetContentPartFieldDefinition(ContentPartFieldDefinition? contentPartFieldDefinition);
    }
}
