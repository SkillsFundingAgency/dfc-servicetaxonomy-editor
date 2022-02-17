using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.JobProfiles.Service.Enums;
using DFC.ServiceTaxonomy.JobProfiles.Service.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.Service.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Service.Services
{
    public class SkillFrameworkBusinessRuleEngine : ISkillFrameworkBusinessRuleEngine
    {
        private readonly ISkillsRepository _skillsOueryRepository;
        private readonly IQueryRepository<FrameworkSkillSuppression> _suppressionsQueryRepository;
        private readonly IQueryRepository<FrameworkContent> _contentReferenceQueryRepository;
        private readonly IQueryRepository<FrameworkSkillCombination> _combinationsQueryRepository;
        private const string MathsTitle = "Mathematics";

        public SkillFrameworkBusinessRuleEngine(
            ISkillsRepository skillsOueryRepository,
            IQueryRepository<FrameworkSkillSuppression> suppressionsQueryRepository,
            IQueryRepository<FrameworkContent> contentReferenceQueryRepository,
            IQueryRepository<FrameworkSkillCombination> combinationsQueryRepository
            )
        {
            _skillsOueryRepository = skillsOueryRepository;
            _suppressionsQueryRepository = suppressionsQueryRepository;
            _contentReferenceQueryRepository = contentReferenceQueryRepository;
            _combinationsQueryRepository = combinationsQueryRepository;
        }

        public IQueryable<OnetSkill> GetAllRawOnetSkillsForOccupation(string onetOccupationalCode)
        {

            var allSkillForOccupation = _skillsOueryRepository.GetAbilitiesForONetOccupationCode(onetOccupationalCode)
                .Union(_skillsOueryRepository.GetKowledgeForONetOccupationCode(onetOccupationalCode)
                .Union(_skillsOueryRepository.GetSkillsForONetOccupationCode(onetOccupationalCode))
                .Union(_skillsOueryRepository.GetWorkStylesForONetOccupationCode(onetOccupationalCode)));

            return allSkillForOccupation;
        }

        /// <summary>
        /// For Knowledge, Skills and Abilities we will have records with LV and IM scales 
        /// Combine in to a single record and average the ranking by adding the LV and IM values and dividing by the number of records
        /// For Work Styles there is just one scale and hence no need to work out an average.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <returns></returns>
        public IEnumerable<OnetSkill> AverageOutscoreScales(IEnumerable<OnetSkill> attributes)
        {

            var averagedScales = attributes.GroupBy(a => new { a.Id, a.Category, a.OnetOccupationalCode, a.Name }).
            Select(c => new OnetSkill
            {
                Id = c.Key.Id,
                Category = c.Key.Category,
                OnetOccupationalCode = c.Key.OnetOccupationalCode,
                Name = c.Key.Name,
                Score = (c.Sum(s => (s.Score)) / c.Count())
            });
            return averagedScales;
        }

        /// <summary>
        /// Any items that have a key length > 7 are at the bottom level so trim them to 7 to move up one node
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public IEnumerable<OnetSkill> MoveBottomLevelAttributesUpOneLevel(IEnumerable<OnetSkill> attributes)
        {
            var bottomNodesUpOne = attributes.Select(a => { a.Id = (a.Id.Length < 7) ? a.Id : a.Id.Substring(0, 7); return a; });
            return bottomNodesUpOne;
        }

        /// <summary>
        /// Remove duplicate attributes, keeping the one with the highest score
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public IEnumerable<OnetSkill> RemoveDuplicateAttributes(IEnumerable<OnetSkill> attributes)
        {
            var duplicatesRemoved = attributes.GroupBy(a => a.Id).Select(b => b.OrderByDescending(c => c.Score).First());
            return duplicatesRemoved;
        }

        /// <summary>
        /// Some attributes appear most job profiles. We have a table that indicated which ones we need to suppres.
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public IEnumerable<OnetSkill> RemoveDFCSuppressions(IEnumerable<OnetSkill> attributes)
        {
            var suppressions = _suppressionsQueryRepository.GetAll().ToList();
            var suppressionsRemoved = attributes.Where(a => !suppressions.Any(s => s.ONetElementId == a.Id));
            return suppressionsRemoved;
        }

        public IEnumerable<OnetSkill> AddTitlesToAttributes(IEnumerable<OnetSkill> attributes)
        {
            var content = _contentReferenceQueryRepository.GetAll();

            var withTitlesAdded = attributes.Join(content, a => a.Id, c => c.ONetElementId,
                (a, c) => new OnetSkill
                {
                    Id = a.Id,
                    Category = a.Category,
                    OnetOccupationalCode = a.OnetOccupationalCode,
                    Name = c.Title,
                    Score = a.Score
                });

            return withTitlesAdded;
        }

        /// <summary>
        /// This rule tries to boost the ranking for Mathematics if it appears under multiple attribute types.
        /// If Mathematics appears for both Skills and Knowledge in our result set.
        /// Delete the  lowest ranking one.
        /// Times the rank for the remaining one by 1.1 adds 10%  (this number was as a result of testing the results) 
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <returns></returns>
        public IEnumerable<OnetSkill> BoostMathsSkills(IEnumerable<OnetSkill> attributes)
        {
            OnetSkill skillsMaths = attributes.FirstOrDefault(a => a.Name == MathsTitle && a.Category == CategoryType.Skill);
            OnetSkill knowledgeMaths = attributes.FirstOrDefault(a => a.Name == MathsTitle && a.Category == CategoryType.Knowledge);

            //if we have both remove one
            if (skillsMaths != null && knowledgeMaths != null)
            {
                if (skillsMaths.Score > knowledgeMaths.Score)
                {
                    attributes = attributes.Where(a => a.Name != MathsTitle || a.Category != CategoryType.Knowledge);
                }
                else
                {
                    attributes = attributes.Where(a => a.Name != MathsTitle || a.Category != CategoryType.Skill);
                }

                var attributeList = attributes.ToList();
                for (int ii = 0; ii < attributeList.Count(); ii++)
                {
                    if (attributeList[ii].Name == MathsTitle)
                    {
                        attributeList[ii].Score = attributeList[ii].Score * 1.1m;
                    }
                }
                return attributeList;
            }
            return attributes;
        }

        /// <summary>
        /// Some times in our top 20 results set we get attributes that are similar - 
        /// We improve user experience by combining similar in the top 20.
        /// Using the combination table we combine simular attributes that are in the top 20 results"
        /// After each combination we need to get a new top 20 of the attributes
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public IEnumerable<OnetSkill> CombineSimilarAttributes(IList<OnetSkill> attributes)
        {
            var combinations = _combinationsQueryRepository.GetAll();

            foreach (var combination in combinations)
            {
                var topAttributes = SelectTopAttributes(attributes);
                OnetSkill elementOne = topAttributes.FirstOrDefault(a => a.Id == combination.OnetElementOneId);
                OnetSkill elementTwo = topAttributes.FirstOrDefault(a => a.Id == combination.OnetElementTwoId);

                //if we have both combine them
                if (elementOne != null && elementTwo != null)
                {
                    var scoreToUse = (elementOne.Score > elementTwo.Score) ? elementOne.Score : elementTwo.Score;

                    attributes = attributes.Where(a => a.Id != elementOne.Id && a.Id != elementTwo.Id).ToList();

                    attributes.Add(new OnetSkill { Name = combination.Title, Category = CategoryType.Combination, Id = combination.CombinedElementId, OnetOccupationalCode = elementOne.OnetOccupationalCode, Score = scoreToUse });
                }
            }
            return attributes;
        }

        public IEnumerable<OnetSkill> SelectFinalAttributes(IEnumerable<OnetSkill> attributes)
        {
            var finalAttributes = attributes.Where(a => a.Category == CategoryType.Combination)
                .Union(SelectTopAttributes(attributes));

            return finalAttributes.OrderByDescending(s => s.Score).Take(20);
        }

        public IEnumerable<OnetSkill> UpdateGDSDescriptions(IEnumerable<OnetSkill> attributes)
        {
            var translations = _skillsOueryRepository.GetAllTranslations();
            foreach (var onetSkill in attributes)
            {
                onetSkill.Description = translations.FirstOrDefault(t => t.ONetElementId == onetSkill.Id)?.Description ?? onetSkill.Name;
            }
            return attributes;
        }


        private static IEnumerable<OnetSkill> SelectTopAttributes(IEnumerable<OnetSkill> attributes)
        {
            var topAttributes = attributes.Where(a => a.Category == CategoryType.Ability).OrderByDescending(s => s.Score).Take(5)
                .Union(attributes.Where(a => a.Category == CategoryType.Knowledge).OrderByDescending(s => s.Score).Take(5))
                .Union(attributes.Where(a => a.Category == CategoryType.Skill).OrderByDescending(s => s.Score).Take(5))
                .Union(attributes.Where(a => a.Category == CategoryType.WorkStyle).OrderByDescending(s => s.Score).Take(5));

            return topAttributes;
        }
    }
}
