using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.Banners.Indexes
{
    public class BannerPartIndex : MapIndex
    {
        public string? ContentItemId { get; set; }
        public string? WebPageName { get; set; }
        public string? WebPageURL { get; set; }
        public bool Latest { get; set; }
        public bool Published { get; set; }
    }
}
