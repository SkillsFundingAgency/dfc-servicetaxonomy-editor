using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public class ContentItemVersion : IContentItemVersion
    {
        public ContentItemVersion(string graphReplicaSetName)
        {
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
    }
}
