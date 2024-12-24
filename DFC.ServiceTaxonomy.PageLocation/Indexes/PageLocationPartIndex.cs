using DFC.ServiceTaxonomy.PageLocation.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.PageLocation.Indexes
{
    public class PageLocationPartIndex : MapIndex
    {
        public string? ContentItemId { get; set; }
        public string? Url { get; set; }
        public string? UrlName { get; set; }
        public bool? UseInTriageTool { get; set; }
    }

    public class PageLocationPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<PageLocationPartIndex>()
                .Map(contentItem =>
                {
                    var url = contentItem.As<PageLocationPart>()?.FullUrl;
                    var urlName = contentItem.As<PageLocationPart>()?.UrlName;

                    if (!string.IsNullOrEmpty(url) && (contentItem.Published || contentItem.Latest))
                    {
                        var content = JsonConvert.SerializeObject(contentItem.Content);
                        var root = JToken.Parse(content);
                        var tile = (bool?)root.SelectToken("..UseInTriageTool.Value");

                        return new PageLocationPartIndex
                        {
                            ContentItemId = contentItem.ContentItemId,
                            Url = url,
                            UrlName = urlName,
                            UseInTriageTool = tile
                        };
                    }

#pragma warning disable CS8603 // Possible null reference return.
                    return null;
#pragma warning restore CS8603 // Possible null reference return.
                });
        }
    }
}
