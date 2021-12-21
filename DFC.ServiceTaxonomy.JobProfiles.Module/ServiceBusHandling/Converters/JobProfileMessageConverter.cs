using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters
{
    internal class JobProfileMessageConverter : IMessageConverter<JobProfileMessage>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageConverter<HowToBecomeData> _howToBecomeMessageConverter;

        public JobProfileMessageConverter(IServiceProvider serviceProvider, IMessageConverter<HowToBecomeData> howToBecomeMessageConverter)
        {
            _serviceProvider = serviceProvider;
            _howToBecomeMessageConverter = howToBecomeMessageConverter;
        }

        public JobProfileMessage ConvertFrom(ContentItem contentItem)
        {
            var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
            List<ContentItem> relatedCareersProfiles = GetContentItems(contentItem.Content.JobProfile.Relatedcareerprofiles, contentManager);

            var jobProfileMessage = new JobProfileMessage
            {
                JobProfileId = contentItem.ContentItemId,
                Title = contentItem.As<TitlePart>().Title,
                WidgetContentTitle = contentItem.Content.JobProfile.WidgetContentTitle.Text,
                AlternativeTitle = contentItem.Content.JobProfile.AlternativeTitle.Text,
                Overview = contentItem.Content.JobProfile.Overview.Text,
                SalaryStarter = contentItem.Content.JobProfile.Salarystarterperyear.Value,
                SalaryExperienced = contentItem.Content.JobProfile.Salaryexperiencedperyear.Value,
                MinimumHours = contentItem.Content.JobProfile.Minimumhours.Value,
                MaximumHours = contentItem.Content.JobProfile.Maximumhours.Value,
                CareerPathAndProgression = contentItem.Content.JobProfile.Careerpathandprogression.Html,
                CourseKeywords = contentItem.Content.JobProfile.Coursekeywords.Text,

                HowToBecomeData = _howToBecomeMessageConverter.ConvertFrom(contentItem),
                RelatedCareersData = GetRelatedCareersData(relatedCareersProfiles)
            };
            return jobProfileMessage;
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
                        Id = contentItem.ContentItemId,
                        Title = contentItem.As<TitlePart>().Title,
                        ProfileLink = contentItem.Content.JobProfile.Url.Text
                    });
                }
            }
            return relatedCareersData;
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
