using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IGraphOperationContext
    {
        ContentItem ContentItem { get; }
        IContentManager ContentManager { get; }
        IContentItemVersion ContentItemVersion { get; }
        ContentTypePartDefinition ContentTypePartDefinition { get; }
        IContentPartFieldDefinition? ContentPartFieldDefinition  { get; }

        IGraphSyncHelper GraphSyncHelper { get; }

        void SetContentPartFieldDefinition(ContentPartFieldDefinition? contentPartFieldDefinition);
    }
}
