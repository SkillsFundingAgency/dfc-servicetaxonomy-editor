using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;
using DFC.ServiceTaxonomy.PageLocation.Models;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OrchardCore.ContentManagement;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters
{
    internal class JobProfileMessageConverter : IMessageConverter<JobProfileMessage>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageConverter<HowToBecomeData> _howToBecomeMessageConverter;
        private readonly IMessageConverter<WhatYouWillDoData> _whatYouWillDoDataMessageConverter;
        private readonly IMessageConverter<WhatItTakesData> _whatItTakesMessageConverter;
        private readonly IMessageConverter<SocCodeItem> _socCodeMessageConverter;
        private readonly ILogger<JobProfileMessageConverter> _logger;

        public JobProfileMessageConverter(
            IMessageConverter<HowToBecomeData> howToBecomeMessageConverter,
            IMessageConverter<WhatYouWillDoData> whatYouWillDoDataMessageConverter,
            IMessageConverter<WhatItTakesData> whatItTakesMessageConverter,
            IMessageConverter<SocCodeItem> socCodeMessageConverter,
            ILogger<JobProfileMessageConverter> logger,
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _howToBecomeMessageConverter = howToBecomeMessageConverter;
            _whatYouWillDoDataMessageConverter = whatYouWillDoDataMessageConverter;
            _whatItTakesMessageConverter = whatItTakesMessageConverter;
            _socCodeMessageConverter = socCodeMessageConverter;
            _logger = logger;
        }

        public async Task<JobProfileMessage> ConvertFromAsync(ContentItem contentItem)
        {
            try
            {
                var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
                IEnumerable<ContentItem> relatedCareersProfiles = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Relatedcareerprofiles, contentManager);
                IEnumerable<ContentItem> dynamicTitlePrefix = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.DynamicTitlePrefix, contentManager);

                var whatItTakesData = await _whatItTakesMessageConverter.ConvertFromAsync(contentItem);

                IEnumerable<ContentItem> workingHoursDetails = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.WorkingHoursDetails, contentManager);
                IEnumerable<ContentItem> workingPatterns = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.WorkingPattern, contentManager);
                IEnumerable<ContentItem> workingPatternDetails = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.WorkingPatternDetails, contentManager);
                IEnumerable<ContentItem> hiddenAlternativeTitle = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.HiddenAlternativeTitle, contentManager);
                IEnumerable<ContentItem> jobProfileSpecialism = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.JobProfileSpecialism, contentManager);
                IEnumerable<ContentItem> jobCategories = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.JobProfileCategory, contentManager);

                var jobProfileMessage = new JobProfileMessage
                {
                    JobProfileId = contentItem.As<GraphSyncPart>().ExtractGuid(),
                    Title = contentItem.As<TitlePart>().Title,
                    UrlName = contentItem.As<PageLocationPart>().UrlName,
                    WidgetContentTitle = contentItem.Content.JobProfile.WidgetContentTitle is null ? default : (string?)contentItem.Content.JobProfile.WidgetContentTitle.Text,
                    AlternativeTitle = contentItem.Content.JobProfile.AlternativeTitle is null ? default : (string?)contentItem.Content.JobProfile.AlternativeTitle.Text,
                    Overview = contentItem.Content.JobProfile.Overview is null ? default : (string?)contentItem.Content.JobProfile.Overview.Text,
                    SalaryStarter = contentItem.Content.JobProfile.Salarystarterperyear is null ? default : (decimal?)contentItem.Content.JobProfile.Salarystarterperyear.Value,
                    SalaryExperienced = contentItem.Content.JobProfile.Salaryexperiencedperyear is null ? default : (decimal?)contentItem.Content.JobProfile.Salaryexperiencedperyear.Value,
                    MinimumHours = contentItem.Content.JobProfile.Minimumhours is null ? default : (decimal?)contentItem.Content.JobProfile.Minimumhours.Value,
                    MaximumHours = contentItem.Content.JobProfile.Maximumhours is null ? default : (decimal?)contentItem.Content.JobProfile.Maximumhours.Value,
                    CareerPathAndProgression = contentItem.Content.JobProfile.Careerpathandprogression is null ? default : (string?)Helper.GetHtmlWithoutInlineCss(contentItem.Content.JobProfile.Careerpathandprogression.Html),
                    CourseKeywords = contentItem.Content.JobProfile.Coursekeywords is null ? default : (string?)contentItem.Content.JobProfile.Coursekeywords.Text,

                    HowToBecomeData = await _howToBecomeMessageConverter.ConvertFromAsync(contentItem),
                    WhatYouWillDoData = await _whatYouWillDoDataMessageConverter.ConvertFromAsync(contentItem),
                    RelatedCareersData = GetRelatedCareersData(relatedCareersProfiles),
                    DynamicTitlePrefix = dynamicTitlePrefix?.FirstOrDefault()?.As<TitlePart>()?.Title ?? string.Empty,

                    WorkingHoursDetails = Helper.MapClassificationData(workingHoursDetails),
                    WorkingPattern = Helper.MapClassificationData(workingPatterns),
                    WorkingPatternDetails = Helper.MapClassificationData(workingPatternDetails),
                    HiddenAlternativeTitle = Helper.MapClassificationData(hiddenAlternativeTitle),
                    JobProfileSpecialism = Helper.MapClassificationData(jobProfileSpecialism),

                    SocSkillsMatrixData = whatItTakesData.RelatedSocSkillMatrixSkills,
                    DigitalSkillsLevel = whatItTakesData.RelatedDigitalSkills,
                    Restrictions = whatItTakesData.RelatedRestrictions,
                    OtherRequirements = whatItTakesData.OtherRequirements,

                    SocCodeData = await _socCodeMessageConverter.ConvertFromAsync(contentItem),
                    IncludeInSitemap = !contentItem.As<SitemapPart>().Exclude,
                    JobProfileCategories = GetJobCategories(jobCategories),
                };

                jobProfileMessage.CanonicalName = !string.IsNullOrEmpty(jobProfileMessage.UrlName) ? jobProfileMessage.UrlName.ToLower() : string.Empty;
                jobProfileMessage.SocLevelTwo = !string.IsNullOrEmpty(jobProfileMessage.SocCodeData?.SOCCode) ? jobProfileMessage.SocCodeData.SOCCode.Substring(0, 2) : string.Empty;

                if (contentItem.ModifiedUtc.HasValue)
                {
                    jobProfileMessage.LastModified = contentItem.ModifiedUtc.Value;
                }
                return jobProfileMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        private static IEnumerable<JobProfileCategoryItem> GetJobCategories(IEnumerable<ContentItem> contentItems)
        {
            return contentItems?.Select(contentItem => new JobProfileCategoryItem
            {
                Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                Title = contentItem.As<TitlePart>().Title,
                Name = contentItem.Content.JobProfileCategory.Description is null ? default : (string?)contentItem.Content.JobProfileCategory.Description.Text
            }) ?? Enumerable.Empty<JobProfileCategoryItem>();
        }

        private static IEnumerable<JobProfileRelatedCareerItem> GetRelatedCareersData(IEnumerable<ContentItem> contentItems) =>
            contentItems?.Select(contentItem => new JobProfileRelatedCareerItem
            {
                Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                Title = contentItem.As<TitlePart>().Title,
                ProfileLink = contentItem.As<PageLocationPart>().FullUrl
            }) ?? Enumerable.Empty<JobProfileRelatedCareerItem>();
    }
}
