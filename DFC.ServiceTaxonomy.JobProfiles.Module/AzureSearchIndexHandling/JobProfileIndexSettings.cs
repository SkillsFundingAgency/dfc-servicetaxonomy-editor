namespace DFC.ServiceTaxonomy.JobProfiles.Module.AzureSearchIndexHandling
{
    internal class JobProfileIndexSettings
    {
        public string? JobProfileSearchIndexName { get; set; }
        public string? SearchServiceName { get; set; }
        public string? SearchServiceAdminAPIKey { get; set; }
        public string? SearchServiceEndPoint { get; set; }
    }
}
