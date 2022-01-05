using System.Threading.Tasks;
using DFC.ServiceTaxonomy.JobProfiles.Service.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Service.Interfaces
{
    public interface ISocMappingRepository
    {
        SocCodeMapping GetById(string id);

        Task<bool> UpdateMappingAsync(string socCode, string onetId);
    }
}
