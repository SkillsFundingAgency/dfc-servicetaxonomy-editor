using OrchardCore.ContentManagement;
using YesSql.Indexes;
using Microsoft.Extensions.Configuration;
using System.Text.Json;


namespace DFC.ServiceTaxonomy.CompUi.Indexes
{
    public class RelatedContentItemIndexProvider : IndexProvider<ContentItem>
    {
        private const string RelatedContentTypesAppSetting = "RelatedContentItemIndexTypes";
        private List<string> RelatedContentTypes;

        public RelatedContentItemIndexProvider(IConfiguration configuration)
        {
            RelatedContentTypes = configuration.GetSection(RelatedContentTypesAppSetting).Get<List<string>>()?.ConvertAll(x => x.ToLower()) ?? new List<string>();
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

                    var content = JsonSerializer.Serialize(contentItem.Content);

                    using var doc = JsonDocument.Parse(content);
                    var root = doc.RootElement;

                    var tiles = new List<string>();

                    var stack = new Stack<JsonElement>();
                    stack.Push(root);

                    while (stack.Count > 0)
                    {
                        var node = stack.Pop();

                        if (node.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var prop in node.EnumerateObject())
                            {
                                if (prop.NameEquals("ContentItemIds") &&
                                    prop.Value.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var item in prop.Value.EnumerateArray())
                                        tiles.Add(item!.Deserialize<string>()!);
                                }

                                stack.Push(prop.Value);
                            }
                        }
                        else if (node.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in node.EnumerateArray())
                                stack.Push(item);
                        }
                    }

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
