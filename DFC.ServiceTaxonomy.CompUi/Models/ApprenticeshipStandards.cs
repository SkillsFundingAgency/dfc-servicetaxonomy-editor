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
        public LARScode? LarsCode { get; set; }
    }

    public class LARScode
    {
        [JsonProperty("Value")]
        public string? Value { get; set; }

        [JsonProperty("Text")]
        public string? Text { get; set; }
    }
}
