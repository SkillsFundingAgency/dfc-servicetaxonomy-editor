using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.CompUi.Models
{
    public class Banners
    {
        [JsonProperty("Pagebanner")]
        public Addabanner? PageBanner { get; set; }
    }

    public class Addabanner
    {
        [JsonProperty("Addabanner")]
        public Items? AddabannerItems { get; set; }
    }

    public class Items
    {
        [JsonProperty("ContentItemIds")]
        public string[]? ContentItemIds { get; set; }
    }
}
