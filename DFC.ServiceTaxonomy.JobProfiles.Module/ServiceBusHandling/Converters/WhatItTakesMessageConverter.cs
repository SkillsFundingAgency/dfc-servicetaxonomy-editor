using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;
using Microsoft.Extensions.DependencyInjection;

using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters
{
    public class WhatItTakesMessageConverter : IMessageConverter<WhatItTakesData>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRelatedSkillsConverter _relatedSkillsConverter;

        public WhatItTakesMessageConverter(IServiceProvider serviceProvider, IRelatedSkillsConverter relatedSkillsConverter)
        {
            _relatedSkillsConverter = relatedSkillsConverter;
            _serviceProvider = serviceProvider;
        }

        public async Task<WhatItTakesData> ConvertFromAsync(ContentItem contentItem)
        {
            var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
            IEnumerable<ContentItem> relatedSocSkillMatrixSkills = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Relatedskills, contentManager);
            IEnumerable<ContentItem> relatedDigitalSkills = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.DigitalSkills, contentManager);
            IEnumerable<ContentItem> relatedRestrictions = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Relatedrestrictions, contentManager);
            var whatItTakesData = new WhatItTakesData
            {
                RelatedSocSkillMatrixSkills = await _relatedSkillsConverter.GetRelatedSkills(relatedSocSkillMatrixSkills),
                RelatedDigitalSkills = GetDigitalSkills(relatedDigitalSkills),
                OtherRequirements = Helper.SanitiseHtml(contentItem.Content.JobProfile.Otherrequirements.Html),
                RelatedRestrictions = GetRestrictions(relatedRestrictions)
            };
            return whatItTakesData;
        }

        private static string GetDigitalSkills(IEnumerable<ContentItem> contentItems) =>
            contentItems?.FirstOrDefault()?.As<TitlePart>()?.Title ?? string.Empty;

        private static IEnumerable<RestrictionItem> GetRestrictions(IEnumerable<ContentItem> contentItems) =>
            contentItems?.Select(contentItem => new RestrictionItem
            {
                Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                Title = contentItem.As<TitlePart>().Title,
                Info = Helper.SanitiseHtmlWithPTag(contentItem.Content.Restriction.Info?.Html)
            }) ?? Enumerable.Empty<RestrictionItem>();
    }
}
