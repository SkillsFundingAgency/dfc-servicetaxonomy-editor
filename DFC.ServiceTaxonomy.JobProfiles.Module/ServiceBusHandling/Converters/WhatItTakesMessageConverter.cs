using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;
using DFC.ServiceTaxonomy.Title.Models;
using Microsoft.Extensions.DependencyInjection;

using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Title.Models;

using YesSql;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters
{
    public class WhatItTakesMessageConverter : IMessageConverter<WhatItTakesData>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISession _session;

        public WhatItTakesMessageConverter(IServiceProvider serviceProvider, ISession session)
        {
            _session = session;
            _serviceProvider = serviceProvider;
        }

        public async Task<WhatItTakesData> ConvertFromAsync(ContentItem contentItem)
        {
            var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
            IEnumerable<ContentItem> relatedSocSkillMatrixSkills = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Relatedskills, contentManager);
            IEnumerable<ContentItem> relatedDigitalSkills = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.DigitalSkills, contentManager);
            IEnumerable<ContentItem> relatedRestrictions = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Relatedrestrictions, contentManager);
            IEnumerable<ContentItem> skillContentItems = await _session.Query<ContentItem, ContentItemIndex>(c => c.ContentType == ContentTypes.Skill && c.Latest).ListAsync();
            IEnumerable<ContentItem> socCodes = await _session.Query<ContentItem, ContentItemIndex>(c => c.ContentType == ContentTypes.SOCCode && c.Latest).ListAsync();
            var whatItTakesData = new WhatItTakesData
            {
                RelatedSocSkillMatrixSkills = GetSkills(relatedSocSkillMatrixSkills, skillContentItems, socCodes),
                RelatedDigitalSkills = GetDigitalSkills(relatedDigitalSkills),
                OtherRequirements = contentItem.Content.JobProfile.Otherrequirements.Html,
                RelatedRestrictions = GetRestrictions(relatedRestrictions)
            };
            return whatItTakesData;
        }

        private IEnumerable<SocSkillMatrixItem> GetSkills(IEnumerable<ContentItem> contentItems, IEnumerable<ContentItem> skillContentItems, IEnumerable<ContentItem> socCodes)
        {
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
                        if(skillContentItem != null)
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
                        if(socCodeContentItem != null)
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


        private static string GetDigitalSkills(IEnumerable<ContentItem> contentItems) =>
            contentItems?.FirstOrDefault()?.As<TitlePart>()?.Title ?? string.Empty;

        private static IEnumerable<RestrictionItem> GetRestrictions(IEnumerable<ContentItem> contentItems) =>
            contentItems?.Select(contentItem => new RestrictionItem
            {
                Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                Title = contentItem.As<TitlePart>().Title,
                Info = contentItem.Content.Restriction.Info?.Html
            }) ?? Enumerable.Empty<RestrictionItem>();
    }
}
