using System;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;
using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters
{
    internal class JobProfileMessageConverter : IMessageConverter<JobProfileMessage>
    {
        private readonly IMessageConverter<HowToBecomeData> _howToBecomeMessageConverter;
        private readonly IMessageConverter<WhatYouWillDoData> _whatYouWillDoDataMessageConverter;

        public JobProfileMessageConverter(IMessageConverter<HowToBecomeData> howToBecomeMessageConverter, IMessageConverter<WhatYouWillDoData> whatYouWillDoDataMessageConverter)
        {
            _howToBecomeMessageConverter = howToBecomeMessageConverter;
            _whatYouWillDoDataMessageConverter = whatYouWillDoDataMessageConverter;
        }

        public JobProfileMessage ConvertFrom(ContentItem contentItem)
        {
            try
            {
                var jobProfileMessage = new JobProfileMessage
                {
                    JobProfileId = contentItem.ContentItemId,
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
            
        }
    }
}
