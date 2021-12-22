using System;
using System.Collections.Generic;
using System.Linq;
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

        public WhatItTakesMessageConverter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public WhatItTakesData ConvertFrom(ContentItem contentItem)
        {
            var contentManager = _serviceProvider.GetRequiredService<IContentManager>();

            //List<ContentItem> relatedSkills = Helper.GetContentItems(contentItem.Content.JobProfile.Relatedskills, contentManager);
            List<ContentItem> relatedDigitalSkills = Helper.GetContentItems(contentItem.Content.JobProfile.Digitalskills, contentManager);
            List<ContentItem> relatedRestrictions = Helper.GetContentItems(contentItem.Content.JobProfile.Relatedrestrictions, contentManager);


            var whatItTakesData = new WhatItTakesData
            {
                //RelatedSkills = GetSkills(relatedSkills),
                RelatedDigitalSkills = GetDigitalSkills(relatedDigitalSkills),
                OtherRequirements = contentItem.Content.JobProfile.Otherrequirements.Html,
                RelatedRestrictions = GetRestrictions(relatedRestrictions)
            };
            return whatItTakesData;
        }

        // TODO: SKILLs
        //private IEnumerable<SkillItem> GetSkills(List<ContentItem> contentItems)
        //{
        //    var skills = new List<SkillItem>();
        //    if (contentItems.Any())
        //    {
        //        foreach (var contentItem in contentItems)
        //        {
        //            skills.Add(new SkillItem
        //            {
        //                Title = contentItem.As<TitlePart>().Title,
        //                Description = contentItem.Content.Skill.Description?.Html,
        //                ONetElementId = contentItem.Content.Skill.Description?.Text
        //            });
        //        }
        //    }

        //    return skills;
        //}

        private string GetDigitalSkills(List<ContentItem> contentItems)
        {
            var digitalSkills = string.Empty;
            if (contentItems.Any())
            {
                var contentItem = contentItems.First();
                if (contentItem != null)
                    digitalSkills = contentItem.As<TitlePart>()?.Title;
            }

            return digitalSkills ?? string.Empty;
        }

        private IEnumerable<RestrictionItem> GetRestrictions(List<ContentItem> contentItems)
        {
            var restrictions = new List<RestrictionItem>();
            if (contentItems.Any())
            {
                foreach (var contentItem in contentItems)
                {
                    restrictions.Add(new RestrictionItem
                    {
                        Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                        Title = contentItem.As<TitlePart>().Title,
                        Info = contentItem.Content.Restriction.Info?.Html
                    });
                }
            }

            return restrictions;
        }
    }
}
