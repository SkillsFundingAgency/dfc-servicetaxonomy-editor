using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.ContentItemVersions
{
    // todo: split out ContentApiBaseUrl and everything else into different classes, and use the contentapi clas in t'other
    public class EscoContentItemVersion : IEscoContentItemVersion
    {
        public string ContentApiBaseUrl => "http://data.europa.eu/esco";

        public VersionOptions VersionOptions => throw new NotImplementedException();
        public (bool? latest, bool? published) ContentItemIndexFilterTerms => throw new NotImplementedException();
        public string GraphReplicaSetName => throw new NotImplementedException();
        public Task<ContentItem> GetContentItem(IContentManager contentManager, string id) => throw new NotImplementedException();
    }
}
