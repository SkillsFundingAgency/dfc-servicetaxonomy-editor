using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Interfaces;
using DFC.ServiceTaxonomy.Title.Models;

using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;

using YesSql;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Converters
{

    public class RelatedSkillsConverter : IRelatedSkillsConverter
    {
        private readonly ISession _session;
        public RelatedSkillsConverter(ISession session)
        {
            _session = session;
        }

        public async Task<IEnumerable<SocSkillMatrixItem>> GetRelatedSkills(IEnumerable<ContentItem> contentItems)
        {
            IEnumerable<ContentItem> skillContentItems = await _session.Query<ContentItem, ContentItemIndex>(c => c.ContentType == ContentTypes.Skill && c.Latest).ListAsync();
            IEnumerable<ContentItem> socCodes = await _session.Query<ContentItem, ContentItemIndex>(c => c.ContentType == ContentTypes.SOCCode && c.Latest).ListAsync();
            var skills = new List<SocSkillMatrixItem>();
            if (contentItems.Any())
            {
                for (int i = 0; i < contentItems.Count(); i++)
                {
                    var contentItem = contentItems.ElementAt(i);
                    var skill = new SocSkillMatrixItem()
                    {
                        Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                        Title = contentItem.As<UniqueTitlePart>()?.Title,
                        Contextualised = contentItem.Content.SOCSkillsMatrix.Contextualised is null ? default : (string?)contentItem.Content.SOCSkillsMatrix.Contextualised.Text,
                        ONetAttributeType = contentItem.Content.SOCSkillsMatrix.ONetAttributeType is null ? default : (string?)contentItem.Content.SOCSkillsMatrix.ONetAttributeType.Text,
                        Rank = i + 1,
                        ONetRank = contentItem.Content.SOCSkillsMatrix.ONetRank is null ? default : (decimal?)contentItem.Content.SOCSkillsMatrix.ONetRank.Text
                    };

                    var skillName = contentItem.Content.SOCSkillsMatrix.RelatedSkill is null ? default : (string?)contentItem.Content.SOCSkillsMatrix.RelatedSkill.Text;

                    if (skillName != null)
                    {
                        var relatedSkill = new FrameworkSkillItem
                        {
                            Title = skillName
                        };
                        var skillContentItem = skillContentItems.FirstOrDefault(c => c.DisplayText == skillName);
                        if (skillContentItem != null)
                        {
                            relatedSkill.Id = skillContentItem.As<GraphSyncPart>().ExtractGuid();
                            relatedSkill.Description = skillContentItem.Content.Skill.Description.Text;
                            relatedSkill.ONetElementId = skillContentItem.Content.Skill.ONetElementId.Text;
                        }

                        skill.RelatedSkill = new List<FrameworkSkillItem> { relatedSkill };
                    }

                    var socCode = contentItem.Content.SOCSkillsMatrix.RelatedSOCcode is null ? default : (string?)contentItem.Content.SOCSkillsMatrix.RelatedSOCcode.Text;
                    if (socCode != null)
                    {
                        var relatedSOCCode = new RelatedSocCodeItem
                        {
                            SOCCode = socCode
                        };
                        var socCodeContentItem = socCodes.FirstOrDefault(c => c.DisplayText == socCode);
                        if (socCodeContentItem != null)
                        {
                            relatedSOCCode.Id = socCodeContentItem.As<GraphSyncPart>().ExtractGuid();
                        }
                        skill.RelatedSOC = new List<RelatedSocCodeItem> { relatedSOCCode };
                    }
                    skills.Add(skill);
                }
            }
            return skills;
        }
    }
}
