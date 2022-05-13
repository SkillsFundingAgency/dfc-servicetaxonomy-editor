using System.Linq;
using DFC.ServiceTaxonomy.JobProfiles.Module.EFDataModels;
using DFC.ServiceTaxonomy.JobProfiles.Module.Enums;
using DFC.ServiceTaxonomy.JobProfiles.Module.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Repositories
{
    public class SkillsOueryRepository : ISkillsRepository
    {
        private readonly DfcDevOnetSkillsFrameworkContext onetDbContext;

        public SkillsOueryRepository(DfcDevOnetSkillsFrameworkContext onetDbContext)
        {
            this.onetDbContext = onetDbContext;
        }

        #region Implementation of ISkillsRepository
        public IQueryable<OnetSkill> GetAbilitiesForONetOccupationCode(string oNetOccupationCode)
        {
            var attributes = from ability in onetDbContext.Abilities.AsQueryable()
                             where ability.RecommendSuppress != "Y"
                                        && ability.NotRelevant != "Y"
                                        && ability.OnetsocCode == oNetOccupationCode
                             select new OnetSkill
                             {
                                 OnetOccupationalCode = ability.OnetsocCode,
                                 Id = ability.ElementId,
                                 Category = CategoryType.Ability,
                                 Score = ability.DataValue,
                             };
            return attributes;
        }

        public IQueryable<OnetSkill> GetKowledgeForONetOccupationCode(string oNetOccupationCode)
        {
            var attributes = from knowledge in onetDbContext.Knowledges.AsQueryable()
                             where knowledge.RecommendSuppress != "Y"
                                 && knowledge.NotRelevant != "Y"
                                 && knowledge.OnetsocCode == oNetOccupationCode

                             select new OnetSkill
                             {
                                 OnetOccupationalCode = knowledge.OnetsocCode,
                                 Id = knowledge.ElementId,
                                 Category = CategoryType.Knowledge,
                                 Score = knowledge.DataValue,
                             };
            return attributes;
        }

        public IQueryable<OnetSkill> GetSkillsForONetOccupationCode(string oNetOccupationCode)
        {
            var attributes = from skill in onetDbContext.Skills.AsQueryable()
                             where skill.RecommendSuppress != "Y"
                                        && skill.NotRelevant != "Y"
                                        && skill.OnetsocCode == oNetOccupationCode
                             select new OnetSkill
                             {
                                 OnetOccupationalCode = skill.OnetsocCode,
                                 Id = skill.ElementId,
                                 Category = CategoryType.Skill,
                                 Score = skill.DataValue,
                             };
            return attributes;
        }

        public IQueryable<OnetSkill> GetWorkStylesForONetOccupationCode(string oNetOccupationCode)
        {
            var attributes = from workStyle in onetDbContext.WorkStyles.AsQueryable()
                             where workStyle.RecommendSuppress != "Y"
                                        && workStyle.OnetsocCode == oNetOccupationCode
                             select new OnetSkill
                             {
                                 OnetOccupationalCode = workStyle.OnetsocCode,
                                 Id = workStyle.ElementId,
                                 Category = CategoryType.WorkStyle,
                                 Score = workStyle.DataValue,
                             };
            return attributes;
        }

        public IQueryable<FrameworkSkill> GetAllTranslations()
        {
            var result = (from trans in onetDbContext.DfcGdstranlations.AsQueryable()
                          join el in onetDbContext.ContentModelReferences on trans.OnetElementId equals el.ElementId
                          where el.ElementId == trans.OnetElementId
                          orderby trans.OnetElementId
                          select new FrameworkSkill
                          {
                              ONetElementId = trans.OnetElementId,
                              Title = el.ElementName,
                              Description = trans.Translation

                          }).Concat(from comb in onetDbContext.DfcGdscombinations.AsQueryable()
                                    orderby comb.CombinedElementId
                                    select new FrameworkSkill
                                    {
                                        ONetElementId = comb.CombinedElementId,
                                        Title = comb.ElementName,
                                        Description = comb.Description
                                    });

            return result;
        }

        #endregion
    }
}
