using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.JobProfiles.Service.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.Service.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Service.Services
{
    public class SkillsFrameworkService : ISkillsFrameworkService
    {
        private readonly ISkillFrameworkBusinessRuleEngine _skillFrameworkBusinessRuleEngine;

        public SkillsFrameworkService(ISkillFrameworkBusinessRuleEngine skillFrameworkBusinessRuleEngine)
        {
            _skillFrameworkBusinessRuleEngine = skillFrameworkBusinessRuleEngine;
        }

        public IEnumerable<OnetSkill> GetRelatedSkillMapping(string onetOccupationalCode)
        {
            //Get All raw attributes linked to occ code from the repository (Skill, knowledge, work styles, ablities)
            var rawAttributes = _skillFrameworkBusinessRuleEngine.GetAllRawOnetSkillsForOccupation(onetOccupationalCode).ToList();

            //Average out the skill thats have LV and LM scales
            var attributes = _skillFrameworkBusinessRuleEngine.AverageOutscoreScales(rawAttributes);
            attributes = _skillFrameworkBusinessRuleEngine.MoveBottomLevelAttributesUpOneLevel(attributes);
            attributes = _skillFrameworkBusinessRuleEngine.RemoveDuplicateAttributes(attributes);
            attributes = _skillFrameworkBusinessRuleEngine.RemoveDFCSuppressions(attributes);
            attributes = _skillFrameworkBusinessRuleEngine.AddTitlesToAttributes(attributes);
            attributes = _skillFrameworkBusinessRuleEngine.BoostMathsSkills(attributes);
            attributes = _skillFrameworkBusinessRuleEngine.CombineSimilarAttributes(attributes.ToList());
            attributes = _skillFrameworkBusinessRuleEngine.SelectFinalAttributes(attributes);

            return attributes;
        }
    }
}
