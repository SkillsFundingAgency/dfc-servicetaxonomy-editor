using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.CompUi.Models
{
    public class SocCode
    {
        [JsonProperty("SOCCode")]
        public SOCCode? SOCCode { get; set; }
    }

    public class SOCCode
    {
        [JsonProperty("ApprenticeshipStandards")]
        public ContentItemIds? ApprenticeshipStandards { get; set; }
    }
}
