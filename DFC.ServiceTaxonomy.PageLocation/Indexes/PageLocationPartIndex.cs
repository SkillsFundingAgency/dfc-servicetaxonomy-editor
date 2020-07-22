using DFC.ServiceTaxonomy.PageLocation.Models;
using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.PageLocation.Indexes
{
    public class PageLocationPartIndex : MapIndex
    {
        public string? ContentItemId { get; set; }
        public string? Url { get; set; }
    }

    public class PageLocationPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<PageLocationPartIndex>()
                .Map(contentItem =>
                {
                    var url = contentItem.As<PageLocationPart>()?.FullUrl;

                    if (!string.IsNullOrEmpty(url) && (contentItem.Published || contentItem.Latest))
                    {
                        return new PageLocationPartIndex
                        {
                            ContentItemId = contentItem.ContentItemId,
                            Url = url
                        };
                    }

#pragma warning disable CS8603 // Possible null reference return.
                    return null;
#pragma warning restore CS8603 // Possible null reference return.
                });
        }
    }
}
