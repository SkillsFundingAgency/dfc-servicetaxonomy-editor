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
            IServiceProvider serviceProvider,
            IMessageConverter<HowToBecomeData> howToBecomeMessageConverter,
            IMessageConverter<WhatYouWillDoData> whatYouWillDoDataMessageConverter,
            IMessageConverter<WhatItTakesData> whatItTakesMessageConverter,
            IMessageConverter<SocCodeItem> socCodeMessageConverter,
            ILogger<JobProfileMessageConverter> logger)
        {
            _serviceProvider = serviceProvider;
            _howToBecomeMessageConverter = howToBecomeMessageConverter;
            _whatYouWillDoDataMessageConverter = whatYouWillDoDataMessageConverter;
            _whatItTakesMessageConverter = whatItTakesMessageConverter;
            _socCodeMessageConverter = socCodeMessageConverter;
            _logger = logger;
        }

        public JobProfileMessage ConvertFrom(ContentItem contentItem)
        {
            try
            {
                var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
                List<ContentItem> relatedCareersProfiles = Helper.GetContentItems(contentItem.Content.JobProfile.Relatedcareerprofiles, contentManager);
                List<ContentItem> dynamicTitlePrefix = Helper.GetContentItems(contentItem.Content.JobProfile.Dynamictitleprefix, contentManager);

                var witMessage = _whatItTakesMessageConverter.ConvertFrom(contentItem);

                List<ContentItem> workingHoursDetails = Helper.GetContentItems(contentItem.Content.JobProfile.WorkingHoursDetails, contentManager);
                List<ContentItem> workingPatterns = Helper.GetContentItems(contentItem.Content.JobProfile.Workingpattern, contentManager);
                List<ContentItem> workingPatternDetails = Helper.GetContentItems(contentItem.Content.JobProfile.Workingpatterndetails, contentManager);
                List<ContentItem> hiddenAlternativeTitle = Helper.GetContentItems(contentItem.Content.JobProfile.HiddenAlternativeTitle, contentManager);
                List<ContentItem> jobProfileSpecialism = Helper.GetContentItems(contentItem.Content.JobProfile.Jobprofilespecialism, contentManager);
                List<ContentItem> jobCategories = Helper.GetContentItems(contentItem.Content.JobProfile.Jobprofilecategory, contentManager);

                var jobProfileMessage = new JobProfileMessage
                {
                    JobProfileId = contentItem.As<GraphSyncPart>().ExtractGuid(),
                    Title = contentItem.As<TitlePart>().Title,
                    UrlName = contentItem.As<PageLocationPart>().FullUrl,
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
                    DynamicTitlePrefix = dynamicTitlePrefix.Any() ? dynamicTitlePrefix.First().As<TitlePart>().Title : string.Empty,

                    WorkingHoursDetails = Helper.MapClassificationData(workingHoursDetails),
                    WorkingPattern = Helper.MapClassificationData(workingPatterns),
                    WorkingPatternDetails = Helper.MapClassificationData(workingPatternDetails),
                    HiddenAlternativeTitle = Helper.MapClassificationData(hiddenAlternativeTitle),
                    JobProfileSpecialism = Helper.MapClassificationData(jobProfileSpecialism),

                    //SocSkillsMatrixData - TODO: RelatedSkills to be added later
                    DigitalSkillsLevel = witMessage.RelatedDigitalSkills,
                    Restrictions = witMessage.RelatedRestrictions,
                    OtherRequirements = witMessage.OtherRequirements,

                    SocCodeData = _socCodeMessageConverter.ConvertFrom(contentItem),
                    IncludeInSitemap = !contentItem.As<SitemapPart>().Exclude,
                    JobProfileCategories = GetJobCategories(jobCategories),
                };

                jobProfileMessage.CanonicalName = jobProfileMessage.UrlName;
                jobProfileMessage.SocLevelTwo = jobProfileMessage.SocCodeData.SOCCode;

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

        private IEnumerable<JobProfileCategoryItem> GetJobCategories(List<ContentItem> contentItems)
        {
            List<JobProfileCategoryItem> jobProfileCategoriesData = new List<JobProfileCategoryItem>();
            if (contentItems.Any())
            {
                foreach (var contentItem in contentItems)
                {
                    jobProfileCategoriesData.Add(new JobProfileCategoryItem
                    {
                        Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                        Title = contentItem.As<TitlePart>().Title,
                        Name = contentItem.Content.Jobprofilecategory.Description == null ? default(string?) : (string?)contentItem.Content.Jobprofilecategory.Description.Text
                    });
                }
            }

            return jobProfileCategoriesData;
        }

        private IEnumerable<JobProfileRelatedCareerItem> GetRelatedCareersData(List<ContentItem> contentItems)
        {
            var relatedCareersData = new List<JobProfileRelatedCareerItem>();
            if (contentItems.Any())
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
    }
}
