using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.CompUi.Models
{
    public class Page
    {
        [JsonProperty("PageLocationPart")]
        public PageLocationParts? PageLocationParts { get; set; }

        [JsonProperty("GraphSyncPart")]
        public GraphSyncParts? GraphSyncParts { get; set; }
    }

    public class PageLocationParts
    {
        [JsonProperty("FullUrl")]
        public string? FullUrl { get; set; }
    }

    public class GraphSyncParts
    {
        [JsonProperty("Text")]
        public string? Text { get; set; }
    }
}
