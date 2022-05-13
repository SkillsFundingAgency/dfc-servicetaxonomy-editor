using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Interfaces
{
    public interface ISkillFrameworkBusinessRuleEngine
    {
        IQueryable<OnetSkill> GetAllRawOnetSkillsForOccupation(string onetOccupationalCode);

        IEnumerable<OnetSkill> AverageOutscoreScales(IEnumerable<OnetSkill> attributes);

        IEnumerable<OnetSkill> MoveBottomLevelAttributesUpOneLevel(IEnumerable<OnetSkill> attributes);

        IEnumerable<OnetSkill> RemoveDuplicateAttributes(IEnumerable<OnetSkill> attributes);

        IEnumerable<OnetSkill> RemoveDFCSuppressions(IEnumerable<OnetSkill> attributes);

        IEnumerable<OnetSkill> AddTitlesToAttributes(IEnumerable<OnetSkill> attributes);

        IEnumerable<OnetSkill> BoostMathsSkills(IEnumerable<OnetSkill> attributes);

        IEnumerable<OnetSkill> CombineSimilarAttributes(IList<OnetSkill> attributes);

        IEnumerable<OnetSkill> SelectFinalAttributes(IEnumerable<OnetSkill> attributes);

        IEnumerable<OnetSkill> UpdateGDSDescriptions(IEnumerable<OnetSkill> attributes);
    }
}
