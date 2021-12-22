using System;
<<<<<<< HEAD
using System.Collections.Generic;
using System.Linq;
=======
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.Extensions;
>>>>>>> 321f7dddee730978bc2f9c6cc94f0b60a18cb91d
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
        private readonly IMessageConverter<WhatYouWillDoData> _whatYouWillDoDataMessageConverter;

<<<<<<< HEAD
        public JobProfileMessageConverter(IServiceProvider serviceProvider, IMessageConverter<HowToBecomeData> howToBecomeMessageConverter)
=======
        public JobProfileMessageConverter(IMessageConverter<HowToBecomeData> howToBecomeMessageConverter, IMessageConverter<WhatYouWillDoData> whatYouWillDoDataMessageConverter)
>>>>>>> 321f7dddee730978bc2f9c6cc94f0b60a18cb91d
        {
            _serviceProvider = serviceProvider;
            _howToBecomeMessageConverter = howToBecomeMessageConverter;
            _whatYouWillDoDataMessageConverter = whatYouWillDoDataMessageConverter;
        }

        public JobProfileMessage ConvertFrom(ContentItem contentItem)
        {
<<<<<<< HEAD
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
=======
            try
            {
                var jobProfileMessage = new JobProfileMessage
                {
                    JobProfileId = contentItem.As<GraphSyncPart>().ExtractGuid(),
                    Title = contentItem.As<TitlePart>().Title,
                    WidgetContentTitle = contentItem.Content.JobProfile.WidgetContentTitle == null ? default(string?) : (string?)contentItem.Content.JobProfile.WidgetContentTitle.Text,
                    AlternativeTitle = contentItem.Content.JobProfile.AlternativeTitle == null ? default(string?) : (string?)contentItem.Content.JobProfile.AlternativeTitle.Text,
                    Overview = contentItem.Content.JobProfile.Overview == null ? default(string?) : (string?)contentItem.Content.JobProfile.Overview.Text,
                    SalaryStarter = contentItem.Content.JobProfile.Salarystarterperyear == null ? default(decimal?) :  (decimal?)contentItem.Content.JobProfile.Salarystarterperyear.Value,
                    SalaryExperienced = contentItem.Content.JobProfile.Salaryexperiencedperyear == null ? default(decimal?) : (decimal?)contentItem.Content.JobProfile.Salaryexperiencedperyear.Value,
                    MinimumHours = contentItem.Content.JobProfile.Minimumhours == null ? default(decimal?) : (decimal?)contentItem.Content.JobProfile.Minimumhours.Value,
                    MaximumHours = contentItem.Content.JobProfile.Maximumhours == null ? default(decimal?) : (decimal?)contentItem.Content.JobProfile.Maximumhours.Value,
                    CareerPathAndProgression = contentItem.Content.JobProfile.Careerpathandprogression == null ? default(string?) : (string?)contentItem.Content.JobProfile.Careerpathandprogression.Html,
                    CourseKeywords = contentItem.Content.JobProfile.Coursekeywords == null ? default(string?) : (string?)contentItem.Content.JobProfile.Coursekeywords.Text,
                    HowToBecomeData = _howToBecomeMessageConverter.ConvertFrom(contentItem),
                    WhatYouWillDoData = _whatYouWillDoDataMessageConverter.ConvertFrom(contentItem)
                };
                return jobProfileMessage;
            }
            catch (Exception ex)
            {
                // TODO : Add Error handling 
                Console.WriteLine(ex.Message);
                throw;
            }
            
>>>>>>> 321f7dddee730978bc2f9c6cc94f0b60a18cb91d
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
