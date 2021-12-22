using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;
using DFC.ServiceTaxonomy.PageLocation.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters
{
    internal class JobProfileMessageConverter : IMessageConverter<JobProfileMessage>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageConverter<HowToBecomeData> _howToBecomeMessageConverter;
        private readonly IMessageConverter<WhatYouWillDoData> _whatYouWillDoDataMessageConverter;
        private readonly IMessageConverter<WhatItTakesData> _whatItTakesMessageConverter;
        private readonly ILogger<JobProfileMessageConverter> _logger;

        public JobProfileMessageConverter(
            IServiceProvider serviceProvider,
            IMessageConverter<HowToBecomeData> howToBecomeMessageConverter,
            IMessageConverter<WhatYouWillDoData> whatYouWillDoDataMessageConverter,
            IMessageConverter<WhatItTakesData> whatItTakesMessageConverter,
            ILogger<JobProfileMessageConverter> logger)
        {
            _serviceProvider = serviceProvider;
            _howToBecomeMessageConverter = howToBecomeMessageConverter;
            _whatYouWillDoDataMessageConverter = whatYouWillDoDataMessageConverter;
            _whatItTakesMessageConverter = whatItTakesMessageConverter;
            _logger = logger;
        }

        public JobProfileMessage ConvertFrom(ContentItem contentItem)
        {
            try
            {
                var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
                List<ContentItem> relatedCareersProfiles = GetContentItems(contentItem.Content.JobProfile.Relatedcareerprofiles, contentManager);
                //List<ContentItem> restrictions = GetContentItems(contentItem.Content.JobProfile.Relatedrestrictions, contentManager);
                List<ContentItem> dynamicTitlePrefix = GetContentItems(contentItem.Content.JobProfile.Dynamictitleprefix, contentManager);
                //List<ContentItem> digitalSkillsLevel = GetContentItems(contentItem.Content.JobProfile.Digitalskills, contentManager);

                var jobProfileMessage = new JobProfileMessage
                {
                    JobProfileId = contentItem.As<GraphSyncPart>().ExtractGuid(),
                    Title = contentItem.As<TitlePart>().Title,
                    WidgetContentTitle = contentItem.Content.JobProfile.WidgetContentTitle == null ? default(string?) : (string?)contentItem.Content.JobProfile.WidgetContentTitle.Text,
                    AlternativeTitle = contentItem.Content.JobProfile.AlternativeTitle == null ? default(string?) : (string?)contentItem.Content.JobProfile.AlternativeTitle.Text,
                    Overview = contentItem.Content.JobProfile.Overview == null ? default(string?) : (string?)contentItem.Content.JobProfile.Overview.Text,
                    SalaryStarter = contentItem.Content.JobProfile.Salarystarterperyear == null ? default(decimal?) : (decimal?)contentItem.Content.JobProfile.Salarystarterperyear.Value,
                    SalaryExperienced = contentItem.Content.JobProfile.Salaryexperiencedperyear == null ? default(decimal?) : (decimal?)contentItem.Content.JobProfile.Salaryexperiencedperyear.Value,
                    MinimumHours = contentItem.Content.JobProfile.Minimumhours == null ? default(decimal?) : (decimal?)contentItem.Content.JobProfile.Minimumhours.Value,
                    MaximumHours = contentItem.Content.JobProfile.Maximumhours == null ? default(decimal?) : (decimal?)contentItem.Content.JobProfile.Maximumhours.Value,
                    CareerPathAndProgression = contentItem.Content.JobProfile.Careerpathandprogression == null ? default(string?) : (string?)contentItem.Content.JobProfile.Careerpathandprogression.Html,
                    CourseKeywords = contentItem.Content.JobProfile.Coursekeywords == null ? default(string?) : (string?)contentItem.Content.JobProfile.Coursekeywords.Text,

                    HowToBecomeData = _howToBecomeMessageConverter.ConvertFrom(contentItem),
                    WhatYouWillDoData = _whatYouWillDoDataMessageConverter.ConvertFrom(contentItem),
                    RelatedCareersData = GetRelatedCareersData(relatedCareersProfiles),
                    //Restrictions = GetRestrictions(restrictions),
                    //OtherRequirements = contentItem.Content.JobProfile.Otherrequirements == null ? default(string?) : (string?)contentItem.Content.JobProfile.Otherrequirements.Html,
                    DynamicTitlePrefix = dynamicTitlePrefix.Any() ? dynamicTitlePrefix.First().As<TitlePart>().Title : string.Empty,
                    //DigitalSkillsLevel = digitalSkillsLevel.Any() ? digitalSkillsLevel.First().Content.Digitalskills.Description.Text : string.Empty,
                   

                    //SocSkillsMatrixData - TODO: RelatedSkills to be added later
                    DigitalSkillsLevel = _whatItTakesMessageConverter.ConvertFrom(contentItem).RelatedDigitalSkills,
                    Restrictions = _whatItTakesMessageConverter.ConvertFrom(contentItem).RelatedRestrictions,
                    OtherRequirements = _whatItTakesMessageConverter.ConvertFrom(contentItem).OtherRequirements
                };
                if(contentItem.ModifiedUtc.HasValue)
                {
                    jobProfileMessage.LastModified = contentItem.ModifiedUtc.Value;
                }
                jobProfileMessage.CanonicalName = contentItem.As<PageLocationPart>().FullUrl;
                return jobProfileMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        private IEnumerable<JobProfileRelatedCareerItem> GetRelatedCareersData(List<ContentItem> contentItems)
        {
            var relatedCareersData = new List<JobProfileRelatedCareerItem>();
            if(contentItems.Any())
            {
                foreach (var contentItem in contentItems)
                {
                    relatedCareersData.Add(new JobProfileRelatedCareerItem
                    {
                        Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                        Title = contentItem.As<TitlePart>().Title,
                        ProfileLink = contentItem.As<PageLocationPart>().FullUrl
                    });
                }
            }
            return relatedCareersData;
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
                        Info = contentItem.Content.Restriction.Info.Html
                    });
                }
            }

            return restrictions;
        }

        private List<ContentItem> GetContentItems(dynamic contentPicker, IContentManager contentManager)
        {
            var contentItemIds = (JArray)contentPicker.ContentItemIds;
            if (contentItemIds.Any())
            {
                var idList = contentItemIds.Select(c => c.Value<string>()).ToList();
                var contentItems = contentManager.GetAsync(idList).Result;
                return contentItems.ToList();
            }

            return new List<ContentItem>();
        }

    }
}
