using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.PageLocation.GraphQL
{
    public class PageItemsPartIndex : MapIndex
    {
        public string? ContentItemId { get; set; }
        public bool? UseInTriageTool { get; set; }
    }
}
