using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces
{
    public interface IRelatedSkillsConverter
    {
        Task<IEnumerable<SocSkillMatrixItem>> GetRelatedSkills(IEnumerable<ContentItem> contentItems);
    }
}
