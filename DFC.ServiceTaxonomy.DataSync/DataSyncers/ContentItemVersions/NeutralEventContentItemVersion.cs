using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.ContentItemVersions
{
    public class NeutralEventContentItemVersion : INeutralEventContentItemVersion
    {
        public string ContentApiBaseUrl => "";

        public VersionOptions VersionOptions => throw new NotImplementedException();
        public (bool? latest, bool? published) ContentItemIndexFilterTerms => throw new NotImplementedException();
        public string DataSyncReplicaSetName => throw new NotImplementedException();
        public Task<ContentItem?> GetContentItem(IContentManager contentManager, string id) => throw new NotImplementedException();
    }
}
