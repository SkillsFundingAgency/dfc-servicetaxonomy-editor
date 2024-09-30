using DFC.ServiceTaxonomy.CompUi.Enums;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.CompUi.Indexes
{
    public class RelatedContentItemIndex : MapIndex
    {
        public string? ContentItemId { get; set; }
        public string? ContentType { get; set; }
        public string? RelatedContentIds { get; set; }
    }

    public class RelatedContentItemIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<RelatedContentItemIndex>()
            .When(contentItem => Enum.IsDefined(typeof(ContentTypes), contentItem.ContentType))
                .Map(contentItem =>
                    {
                        if (!contentItem.Published && !contentItem.Latest)
                        {
                            return default!;
                        }

                        var content = JsonConvert.SerializeObject(contentItem.Content);

                        var root = JToken.Parse(content);

                        var tiles = root.SelectTokens("..ContentItemIds[*]");

                        string contentIdList = string.Join("','", tiles);
                        if (!string.IsNullOrEmpty(contentIdList))
                        {
                            contentIdList = string.Concat("'", contentIdList, "'");
                        }
                        else
                        {
                            contentIdList = string.Empty;
                        }

                        return new RelatedContentItemIndex
                        {
                            ContentItemId = contentItem.ContentItemId,
                            ContentType = contentItem.ContentType,
                            RelatedContentIds = contentIdList,
                        };
                    });
        }
    }
}
