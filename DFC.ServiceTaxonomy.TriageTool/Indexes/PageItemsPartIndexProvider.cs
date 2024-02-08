using DFC.ServiceTaxonomy.PageLocation.GraphQL;
using DFC.ServiceTaxonomy.PageLocation.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using YesSql.Indexes;


namespace DFC.ServiceTaxonomy.PageLocation.Indexes
{

    public class PageItemsPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {

            context.For<PageItemsPartIndex>()
                .Map(contentItem =>
                {
                   return PagePartTest(contentItem);

                   /* var useInTriageTool = contentItem.As<PagePart>()?.UseInTriageTool;

                    if ((contentItem.Published || contentItem.Latest))
                    {
                        return new PageItemsPartIndex
                        {
                            ContentItemId = contentItem.ContentItemId,
                            UseInTriageTool = useInTriageTool

                        };
                    }

#pragma warning disable CS8603 // Possible null reference return.
                    return null;
#pragma warning restore CS8603 // Possible null reference return.*/
                });
        }

        public PageItemsPartIndex PagePartTest(ContentItem contentItem)
        {

            var useInTriageTool = contentItem.As<PagePart>()?.UseInTriageTool;

            if ((contentItem.Published || contentItem.Latest))
            {
               
                var content = JsonConvert.SerializeObject(contentItem.Content);
                var root = JToken.Parse(content);
                var tile = (bool?) root.SelectToken("..UseInTriageTool.Value");


                var index = new PageItemsPartIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    UseInTriageTool = tile,
                };
                return index;
            }

#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }

    
}
