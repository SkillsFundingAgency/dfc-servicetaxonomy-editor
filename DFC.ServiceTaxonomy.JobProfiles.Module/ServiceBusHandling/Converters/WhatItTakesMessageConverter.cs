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
            IEnumerable<ContentItem> relatedDigitalSkills = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Digitalskills, contentManager);
            IEnumerable<ContentItem> relatedRestrictions = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Relatedrestrictions, contentManager);

            var whatItTakesData = new WhatItTakesData
            {
                RelatedSocSkillMatrixSkills = GetSkillsTwo(relatedSocSkillMatrixSkills),
                RelatedDigitalSkills = GetDigitalSkills(relatedDigitalSkills),
                OtherRequirements = contentItem.Content.JobProfile.Otherrequirements.Html,
                RelatedRestrictions = GetRestrictions(relatedRestrictions)
            };
            return whatItTakesData;
        }

        private static IEnumerable<SocSkillMatrixItem> GetSkillsTwo(IEnumerable<ContentItem> contentItems)
        {
            var skills = new List<SocSkillMatrixItem>();
            if (contentItems.Any())
            {
                foreach (var contentItem in contentItems)
                {
                    skills.Add(new SocSkillMatrixItem
                    {
                        Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                        Title = contentItem.As<UniqueTitlePart>()?.Title,
                        Contextualised = contentItem.Content.SOCskillsmatrix.Contextualised is null ? default : (string?)contentItem.Content.SOCskillsmatrix.Contextualised.Text,
                        ONetAttributeType = contentItem.Content.SOCskillsmatrix.ONetAttributeType is null ? default : (string?)contentItem.Content.SOCskillsmatrix.ONetAttributeType.Text,
                        Rank = contentItem.Content.SOCskillsmatrix.Rank is null ? default : (decimal?)contentItem.Content.SOCskillsmatrix.Rank.Rank,
                        ONetRank = contentItem.Content.SOCskillsmatrix.ONetRank is null ? default : (decimal?)contentItem.Content.SOCskillsmatrix.ONetRank.Text
                    });
                    var skillName = contentItem.Content.SOCskillsmatrix.RelatedSkill is null ? default : (decimal?)contentItem.Content.SOCskillsmatrix.RelatedSkill.Text;
                    var socCode = contentItem.Content.SOCskillsmatrix.RelatedSOCCode is null ? default : (decimal?)contentItem.Content.SOCskillsmatrix.RelatedSOCCode.Text
                }
            }
            return skills; 
        }
        // TODO: SKILLs
        private static IEnumerable<SocSkillMatrixItem> GetSkills(IEnumerable<ContentItem> contentItems) =>
            contentItems?.Select(contentItem => new SocSkillMatrixItem
            {
                Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                Title = contentItem.As<UniqueTitlePart>()?.Title,
                Contextualised = contentItem.Content.SOCskillsmatrix.Contextualised is null ? default : (string?)contentItem.Content.SOCskillsmatrix.Contextualised.Text,
                ONetAttributeType = contentItem.Content.SOCskillsmatrix.OnetAttributeType is null ? default : (string?)contentItem.Content.SOCskillsmatrix.OnetAttributeType.Text,
                Rank = contentItem.Content.SOCskillsmatrix.Rank is null ? default : (decimal?)contentItem.Content.SOCskillsmatrix.Rank.Text,
                ONetRank = contentItem.Content.SOCskillsmatrix.ONetRank is null ? default : (decimal?)contentItem.Content.SOCskillsmatrix.ONetRank.Text
            }) ?? Enumerable.Empty<SocSkillMatrixItem>();

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
