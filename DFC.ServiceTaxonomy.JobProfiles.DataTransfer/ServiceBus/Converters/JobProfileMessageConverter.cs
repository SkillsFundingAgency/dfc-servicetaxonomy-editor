using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Interfaces;
using DFC.ServiceTaxonomy.PageLocation.Models;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore;
using OrchardCore.ContentManagement;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Converters
{
    internal class JobProfileMessageConverter : IMessageConverter<JobProfileMessage>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOrchardHelper _orchardHelper;
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
            IServiceProvider serviceProvider,
            IOrchardHelper orchardHelper)
        {
            _serviceProvider = serviceProvider;
            _orchardHelper = orchardHelper;
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

                Thumbnail? videoThumbnail = null;

                if (contentItem.Content.JobProfile.VideoThumbnail.Paths is not null && contentItem.Content.JobProfile.VideoThumbnail.Paths.Count > 0)
                {
                    string assetPath = contentItem.Content.JobProfile.VideoThumbnail.Paths[0];
                    videoThumbnail = new Thumbnail(
                    url: _orchardHelper.AssetUrl(assetPath, width: 600),
                        text: (string)contentItem.Content.JobProfile.VideoThumbnail.MediaTexts[0]
                    );
                }

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
                    CareerPathAndProgression = contentItem.Content.JobProfile.Careerpathandprogression is null ? default : (string?)contentItem.Content.JobProfile.Careerpathandprogression.Html,
                    CourseKeywords = contentItem.Content.JobProfile.Coursekeywords is null ? default : (string?)contentItem.Content.JobProfile.Coursekeywords.Text,

                    Video = (((string?)contentItem.Content.JobProfile.VideoType.Text ?? "None") == "None") ? default : new SocialProofVideo(
                        type: (string)contentItem.Content.JobProfile.VideoType.Text,
                        title: (string)contentItem.Content.JobProfile.VideoTitle.Text,
                        summaryHtml: (string)contentItem.Content.JobProfile.VideoSummary.Html,
                        thumbnail: videoThumbnail,
                        furtherInformationHtml: (string)contentItem.Content.JobProfile.VideoFurtherInformation.Html,
                        url: (string)contentItem.Content.JobProfile.VideoUrl.Text,
                        linkText: (string)contentItem.Content.JobProfile.VideoLinkText.Text,
                        duration: (string)contentItem.Content.JobProfile.VideoDuration.Text,
                        transcript: (string)contentItem.Content.JobProfile.VideoTranscript.Text
                    ),

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

        private static IEnumerable<JobProfileCategoryItem> GetJobCategories(IEnumerable<ContentItem> contentItems) =>
            contentItems?.Select(contentItem => new JobProfileCategoryItem
            {
                Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                Title = contentItem.As<TitlePart>().Title,
                Name = contentItem.Content.JobProfileCategory.Description is null ? default : (string?)contentItem.Content.JobProfileCategory.Description.Text
            }) ?? Enumerable.Empty<JobProfileCategoryItem>();

        private static IEnumerable<JobProfileRelatedCareerItem> GetRelatedCareersData(IEnumerable<ContentItem> contentItems) =>
            contentItems?.Select(contentItem => new JobProfileRelatedCareerItem
            {
                Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                Title = contentItem.As<TitlePart>().Title,
                ProfileLink = contentItem.As<PageLocationPart>().FullUrl
            }) ?? Enumerable.Empty<JobProfileRelatedCareerItem>();
    }
}
