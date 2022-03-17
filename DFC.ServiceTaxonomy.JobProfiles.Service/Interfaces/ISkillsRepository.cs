using System.Linq;
using DFC.ServiceTaxonomy.JobProfiles.Service.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Service.Interfaces
{
    public interface ISkillsRepository
    {
        IQueryable<OnetSkill> GetSkillsForONetOccupationCode(string oNetOccupationCode);

        IQueryable<OnetSkill> GetAbilitiesForONetOccupationCode(string oNetOccupationCode);

        IQueryable<OnetSkill> GetKowledgeForONetOccupationCode(string oNetOccupationCode);

        IQueryable<OnetSkill> GetWorkStylesForONetOccupationCode(string oNetOccupationCode);

        IQueryable<FrameworkSkill> GetAllTranslations();
    }
}
