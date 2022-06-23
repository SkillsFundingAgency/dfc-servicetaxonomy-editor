using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch
{
    public interface IReIndexService
    {
        Task ReIndexAsync();
    }
}
