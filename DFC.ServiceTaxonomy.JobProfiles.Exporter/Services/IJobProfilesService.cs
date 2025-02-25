namespace DFC.ServiceTaxonomy.JobProfiles.Exporter.Services
{
    public interface IJobProfilesService
    {
        Task<List<string>> GetAllJobProfilesUrls();

        Task<MemoryStream> GenerateCsvStreamAsync();
    }
}
