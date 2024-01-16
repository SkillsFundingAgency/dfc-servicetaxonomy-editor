using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.CompUi.Models
{
    public class Page
    {
        [JsonProperty("PageLocationPart")]
        public PageLocationParts? PageLocationParts { get; set; }
    }

    public class PageLocationParts
    {
        [JsonProperty("FullUrl")]
        public string? FUllUrl { get; set; }
    }
}
