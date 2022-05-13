using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Interfaces
{
    public interface IRelatedSkillsConverter
    {
        Task<IEnumerable<SocSkillMatrixItem>> GetRelatedSkills(IEnumerable<ContentItem> contentItems);
    }
}
