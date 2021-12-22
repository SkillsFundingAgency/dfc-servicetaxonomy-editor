﻿using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;
using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters
{
    internal class JobProfileMessageConverter : IMessageConverter<JobProfileMessage>
    {
        private readonly IMessageConverter<HowToBecomeData> _howToBecomeMessageConverter;
        private readonly IMessageConverter<WhatItTakesData> _whatItTakesMessageConverter;

        public JobProfileMessageConverter(IMessageConverter<HowToBecomeData> howToBecomeMessageConverter,
                                          IMessageConverter<WhatItTakesData> whatItTakesMessageConverter)
        {
            _howToBecomeMessageConverter = howToBecomeMessageConverter;
            _whatItTakesMessageConverter = whatItTakesMessageConverter;
        }

        public JobProfileMessage ConvertFrom(ContentItem contentItem)
        {
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

                //SocSkillsMatrixData
                DigitalSkillsLevel = _whatItTakesMessageConverter.ConvertFrom(contentItem).RelatedDigitalSkills,
                Restrictions = _whatItTakesMessageConverter.ConvertFrom(contentItem).RelatedRestrictions,
                OtherRequirements = _whatItTakesMessageConverter.ConvertFrom(contentItem).OtherRequirements
            };


            return jobProfileMessage;
        }
    }
}
