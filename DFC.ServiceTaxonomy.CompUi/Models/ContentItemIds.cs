using System.Text.Json.Serialization;

namespace DFC.ServiceTaxonomy.CompUi.Models
{
    public class ContentItemIds
    {
        [JsonPropertyName("ContentItemIds")]
        public string[]? ContentItemId { get; set; }
    }
}
