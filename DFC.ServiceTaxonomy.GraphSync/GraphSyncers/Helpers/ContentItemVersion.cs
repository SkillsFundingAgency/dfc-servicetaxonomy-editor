using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public class ContentItemVersion : IContentItemVersion
    {
        //todo: private ctor that takes 3 args?
        private static ContentItemVersion Published => new ContentItemVersion("published");
        private static ContentItemVersion Draft => new ContentItemVersion("draft");

        public ContentItemVersion(string graphReplicaSetName)
        {
            GraphReplicaSetName = graphReplicaSetName;
            switch (graphReplicaSetName)
            {
                case "published":
                    VersionOptions = VersionOptions.Published;
                    ContentItemIndexFilterTerms = (null, true);
                    break;
                case "draft":
                    VersionOptions = VersionOptions.Draft;
                    ContentItemIndexFilterTerms = (true, false);
                    break;
                default:
                    throw new GraphSyncException($"Unknown graph replica set '{graphReplicaSetName}'.");
            }
        }

        public string GraphReplicaSetName { get; }
        public VersionOptions VersionOptions { get; }

        // ContentItem properties according to item version
        //  Latest  Published
        //     1        0        draft only
        //     1        1        published only
        //     0        1        published (with separate draft version)
        //     1        0        draft (with separate published version)
        //     0        0        old (replaced) published version
        //     1        1        new (replacement) published version
        //     <Not Kept>        old (replaced) draft version
        //     1        0        new (replacement) draft version
        public (bool? latest, bool published) ContentItemIndexFilterTerms { get; }

        //todo: static Published and Draft ContentItemVersions?
        // with GraphReplicaSetName property, used in ToString?
        // public override string ToString()
        // {
        //     return
        // }

        //todo: the structure of this class?
        public async Task<ContentItem> GetContentItemAsync(IContentManager contentManager, string id)
        {
            ContentItem? contentItem = null;
            if (GraphReplicaSetName == "draft")
                contentItem = await contentManager.GetAsync(id, VersionOptions.Draft);

            return contentItem ?? await contentManager.GetAsync(id, VersionOptions.Published);
        }
    }
}
