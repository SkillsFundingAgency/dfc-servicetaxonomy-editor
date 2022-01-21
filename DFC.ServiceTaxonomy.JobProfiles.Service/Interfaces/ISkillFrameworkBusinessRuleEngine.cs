using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.JobProfiles.Service.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Service.Interfaces
{
    public interface ISkillFrameworkBusinessRuleEngine
    {
        //DigitalSkillsLevel GetDigitalSkillsLevel(int count);

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
