using System.Text.Json;
using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.CompUi.Models
{
    public class ContentItemIds
    {
        [JsonProperty("ContentItemIds")]
        public string[]? ContentItemId { get; set; }
    }
}
