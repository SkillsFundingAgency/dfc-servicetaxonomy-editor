using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.PageLocation.Indexes
{
    public class JobProfileSimplificationPartIndex : MapIndex
    {
        public string? ContentItemId { get; set; }
        public string? VideoThumbnailText { get; set; }
    }

    public class JobProfileSimplificationPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<JobProfileSimplificationPartIndex>()
                .Map(contentItem =>
                {
                    if (contentItem.Published || contentItem.Latest)
                    {
                        var content = JsonConvert.SerializeObject(contentItem.Content);
                        var root = JToken.Parse(content);
                        var tile = (string?)root.SelectToken("..VideoThumbnail.MediaTexts");


                        return new JobProfileSimplificationPartIndex
                        {
                            ContentItemId = contentItem.ContentItemId,
                            VideoThumbnailText = tile
                        };
                    }

#pragma warning disable CS8603 // Possible null reference return.
                    return null;
#pragma warning restore CS8603 // Possible null reference return.
                });
        }
    }
}
