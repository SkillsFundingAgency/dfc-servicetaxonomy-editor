using System.Linq;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Interfaces
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
