using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.CompUi.Models
{
    public class ApprenticeshipStandards
    {
        [JsonProperty("ApprenticeshipStandard")]
        public ApprenticeshipStandard? ApprenticeshipStandard { get; set; }
    }

    public class ApprenticeshipStandard
    {
        [JsonProperty("LARScode")]
        public LARScode? Description { get; set; }
    }

    public class LARScode
    {
        [JsonProperty("Text")]
        public string? Text { get; set; }
    }
}
