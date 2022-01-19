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
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters
{
    public class WhatItTakesMessageConverter : IMessageConverter<WhatItTakesData>
    {
        private readonly IServiceProvider _serviceProvider;

        public WhatItTakesMessageConverter(IServiceProvider serviceProvider) =>
            _serviceProvider = serviceProvider;

        public async Task<WhatItTakesData> ConvertFromAsync(ContentItem contentItem)
        {
            var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
            IEnumerable<ContentItem> relatedSocSkillMatrixSkills = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Relatedskills, contentManager);
            IEnumerable<ContentItem> relatedDigitalSkills = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.DigitalSkills, contentManager);
            IEnumerable<ContentItem> relatedRestrictions = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Relatedrestrictions, contentManager);

            var whatItTakesData = new WhatItTakesData
            {
                RelatedSocSkillMatrixSkills = GetSkills(relatedSocSkillMatrixSkills),
                RelatedDigitalSkills = GetDigitalSkills(relatedDigitalSkills),
                OtherRequirements = contentItem.Content.JobProfile.Otherrequirements.Html,
                RelatedRestrictions = GetRestrictions(relatedRestrictions)
            };
            return whatItTakesData;
        }

        private IEnumerable<SocSkillMatrixItem> GetSkills(IEnumerable<ContentItem> contentItems)
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
                        // TODO: If we can provide the Rank/Ordinal this code can be changed
                        Rank = i + 1, // contentItem.Content.SOCSkillsMatrix.Rank is null ? default : (decimal?)contentItem.Content.SOCSkillsMatrix.Rank.Rank,
                        ONetRank = contentItem.Content.SOCSkillsMatrix.ONetRank is null ? default : (decimal?)contentItem.Content.SOCSkillsMatrix.ONetRank.Text
                    };

                    var skillName = contentItem.Content.SOCSkillsMatrix.RelatedSkill is null ? default : (string?)contentItem.Content.SOCSkillsMatrix.RelatedSkill.Text;
                    if (skillName != null)
                    {
                        var relatedSkills = new List<FrameworkSkillItem>()
                        {
                            new FrameworkSkillItem()
                            {
                                // TODO: If absence of Guid ID or other field cause any issue the code will be changed 
                                //Id = GuidId,
                                Title = skillName
                            }
                        };
                        skill.RelatedSkill = relatedSkills;
                    }

                    var socCode = contentItem.Content.SOCSkillsMatrix.RelatedSOCcode is null ? default : (string?)contentItem.Content.SOCSkillsMatrix.RelatedSOCcode.Text;
                    if (socCode != null)
                    {
                        var relatedSOCCodes = new List<RelatedSocCodeItem>()
                        {
                            new RelatedSocCodeItem()
                            {
                                // TODO: If absence of Guid ID or other field cause any issue the code will be changed 
                                //Id = GuidId,
                                SOCCode = socCode
                            }
                        };
                        skill.RelatedSOC = relatedSOCCodes;
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
