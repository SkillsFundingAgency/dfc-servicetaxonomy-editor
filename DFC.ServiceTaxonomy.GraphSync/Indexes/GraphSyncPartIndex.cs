using DFC.ServiceTaxonomy.GraphSync.Models;
using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.GraphSync.Indexes
{
    public class GraphSyncPartIndex : MapIndex
    {
        public string? ContentItemId { get; set; }
        public string? NodeId { get; set; }
    }

    public class GraphSyncPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<GraphSyncPartIndex>()
                .Map(contentItem =>
                {
                    var text = contentItem.As<GraphSyncPart>()?.Text;

                    if (!string.IsNullOrEmpty(text) && (contentItem.Published || contentItem.Latest))
                    {
                        return new GraphSyncPartIndex {ContentItemId = contentItem.ContentItemId, NodeId = text};
                    }

#pragma warning disable CS8603 // Possible null reference return.
                    return null;
#pragma warning restore CS8603 // Possible null reference return.
                });
        }
    }
}
