using System.Diagnostics.CodeAnalysis;

namespace DFC.ServiceTaxonomy.Content.Configuration
{
    [ExcludeFromCodeCoverage]
    public class AzureAdSettings
    {
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? Authority { get; set; }
        public string? SubscriptionId { get; set; }
        public string? CdnProfileName { get; set; }
        public string? CdnEndpointName { get; set; }
        public string? ResourceGroupName { get; set; }
        public string? KeyVaultAddress { get; set; }
    }
}
