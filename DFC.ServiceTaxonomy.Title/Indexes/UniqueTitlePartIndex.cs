using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.Title.Indexes
{
    public class UniqueTitlePartIndex : MapIndex
    {
        public string? ContentItemId { get; set; }
        public string? Title { get; set; }
        public bool Latest { get; set; }
        public bool Published { get; set; }
    }
}
