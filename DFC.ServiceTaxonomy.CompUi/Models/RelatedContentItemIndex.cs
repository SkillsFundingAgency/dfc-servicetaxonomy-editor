using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.CompUi.Models
{
    public class RelatedContentItemIndex : MapIndex
    {
        public string? ContentItemId { get; set; }
        public string? ContentType { get; set; }
        public string? RelatedContentIds { get; set; }
    }
}
