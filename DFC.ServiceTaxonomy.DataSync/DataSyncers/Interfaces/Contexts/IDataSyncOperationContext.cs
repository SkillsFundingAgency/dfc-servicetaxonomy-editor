using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.OrchardCore.Interfaces;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts
{
    public interface IDataSyncOperationContext
    {
        ContentItem ContentItem { get; }
        IContentManager ContentManager { get; }
        IContentItemVersion ContentItemVersion { get; }
        ContentTypePartDefinition ContentTypePartDefinition { get; }
        IContentPartFieldDefinition? ContentPartFieldDefinition  { get; }

        ISyncNameProvider SyncNameProvider { get; }

        void SetContentPartFieldDefinition(ContentPartFieldDefinition? contentPartFieldDefinition);
    }
}
