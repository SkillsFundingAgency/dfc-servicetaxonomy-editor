using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.CompUi.Models
{
    public class SharedContent
    {
        [JsonProperty("GraphSyncPart")]
        public SharedContentText? SharedContentText { get; set; }
    }

    public class SharedContentText
    {
        [JsonProperty("Text")]
        public string? NodeId { get; set; }
    }
}
