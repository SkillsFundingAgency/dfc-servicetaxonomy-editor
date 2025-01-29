using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using OrchardCore.ContentManagement;
using YesSql.Indexes;
using Microsoft.Extensions.Configuration;


namespace DFC.ServiceTaxonomy.CompUi.Indexes
{
    public class RelatedContentItemIndexProvider : IndexProvider<ContentItem>
    {
        private const string RelatedContentTypesAppSetting = "RelatedContentItemIndexTypes";
        private List<string> RelatedContentTypes;

        public RelatedContentItemIndexProvider(IConfiguration configuration)
        {
            RelatedContentTypes = configuration.GetSection(RelatedContentTypesAppSetting).Get<List<string>>()?.ConvertAll(x =>x.ToLower()) ?? new List<string>();
        }

        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<Models.RelatedContentItemIndex>()
             .When(contentItem => RelatedContentTypes.Contains(contentItem.ContentType.ToLower()))
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

                        return new Models.RelatedContentItemIndex
                        {
                            ContentItemId = contentItem.ContentItemId,
                            ContentType = contentItem.ContentType,
                            RelatedContentIds = contentIdList,
                        };
                    });
        }
    }
}
