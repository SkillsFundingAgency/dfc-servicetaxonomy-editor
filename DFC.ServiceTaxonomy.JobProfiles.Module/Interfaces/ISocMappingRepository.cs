using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Interfaces
{
    public interface ISocMappingRepository
    {
        Task<bool> UpdateMappingAsync(string socCode, string onetId);
    }
}
