using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.CompUi.Models
{
    public class PageBanners
    {
        [JsonProperty("BannerPart")]
        public BannerParts? BannerParts { get; set; }

    }
    public class BannerParts
    {
        [JsonProperty("WebPageName")]
        public string? WebPageName { get; set; }

        [JsonProperty("WebPageURL")]
        public string? WebPageUrl { get; set; }
    }
}

