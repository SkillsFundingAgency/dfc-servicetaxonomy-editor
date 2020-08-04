using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Models;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IValidateAndRepairContext
    {
        IContentManager ContentManager { get; }
        IContentItemVersion ContentItemVersion { get; }
        ContentTypePartDefinition ContentTypePartDefinition { get; }
        IContentPartFieldDefinition? ContentPartFieldDefinition  { get; }

        INodeWithOutgoingRelationships NodeWithOutgoingRelationships { get; }
        IGraphSyncHelper GraphSyncHelper { get; }
        IGraphValidationHelper GraphValidationHelper { get; }
        IDictionary<string, int> ExpectedRelationshipCounts { get; }
        IValidateAndRepairGraph ValidateAndRepairGraph { get; }

        void SetContentPartFieldDefinition(ContentPartFieldDefinition? contentPartFieldDefinition);
    }
}
