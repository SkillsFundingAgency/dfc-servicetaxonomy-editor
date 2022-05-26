using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.AzureSearch;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Converters;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Interfaces;
using DFC.ServiceTaxonomy.PageLocation.Models;

using Microsoft.Extensions.DependencyInjection;

using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;


namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch.Converters
{
    public class JobProfileIndexMessageConverter : IMessageConverter<JobProfileIndex>
    {
        private readonly IServiceProvider _serviceProvider;

        public JobProfileIndexMessageConverter(IServiceProvider serviceProvider) =>
            _serviceProvider = serviceProvider;

        public async Task<JobProfileIndex> ConvertFromAsync(ContentItem contentItem)
        {
            var contentManager = _serviceProvider.GetRequiredService<IContentManager>();

            IEnumerable<string> socCode = await Helper.GetContentItemNamesAsync(contentItem.Content.JobProfile.SOCCode, contentManager);
            IEnumerable<ContentItem> jobCategories = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.JobProfileCategory, contentManager);

            var jobProfileIndex = new JobProfileIndex();
            jobProfileIndex.IdentityField = contentItem.As<GraphSyncPart>().ExtractGuid().ToString();
            jobProfileIndex.SocCode = socCode.FirstOrDefault();
            jobProfileIndex.Title = contentItem.As<TitlePart>().Title;
            string altText = string.IsNullOrEmpty(contentItem.Content.JobProfile.AlternativeTitle.Text.ToString()) ? string.Empty : contentItem.Content.JobProfile.AlternativeTitle.Text.ToString();
            jobProfileIndex.AlternativeTitle = altText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
            jobProfileIndex.Overview = contentItem.Content.JobProfile.Overview.Text ?? string.Empty;
            jobProfileIndex.SalaryStarter = string.IsNullOrEmpty(contentItem.Content.JobProfile.Salarystarterperyear.Value.ToString()) ? default : (double)contentItem.Content.JobProfile.Salarystarterperyear.Value;
            jobProfileIndex.SalaryExperienced = string.IsNullOrEmpty(contentItem.Content.JobProfile.Salaryexperiencedperyear.Value.ToString()) ? default : (double)contentItem.Content.JobProfile.Salaryexperiencedperyear.Value;
            jobProfileIndex.UrlName = contentItem.As<PageLocationPart>().UrlName ?? string.Empty;
            jobProfileIndex.JobProfileCategories = await Helper.GetContentItemNamesAsync(contentItem.Content.JobProfile.JobProfileCategory, contentManager);
            jobProfileIndex.JobProfileSpecialism = await Helper.GetContentItemNamesAsync(contentItem.Content.JobProfile.JobProfileSpecialism, contentManager);
            jobProfileIndex.HiddenAlternativeTitle = await Helper.GetContentItemNamesAsync(contentItem.Content.JobProfile.HiddenAlternativeTitle, contentManager);
            jobProfileIndex.JobProfileCategoriesWithUrl = GetJobCategoriesWithUrl(jobCategories);
            jobProfileIndex.JobProfileCategoryUrls = GetJobCategoryUrls(jobCategories);

            // These are search enablers from legacy Sitefinity functionality and are not used anymore but kept here so it does not break anything
            // These can be removed later if not required.
            jobProfileIndex.Interests = new List<string>();
            jobProfileIndex.Enablers = new List<string>();
            jobProfileIndex.EntryQualifications = new List<string>();
            jobProfileIndex.TrainingRoutes = new List<string>();
            jobProfileIndex.PreferredTaskTypes = new List<string>();
            jobProfileIndex.JobAreas = new List<string>();
            jobProfileIndex.EntryQualificationLowestLevel = default;

            jobProfileIndex.Skills = await Helper.GetRelatedSkillsAsync(contentItem.Content.JobProfile.Relatedskills, contentManager);

            jobProfileIndex.CollegeRelevantSubjects = GetHtml(contentItem.Content.JobProfile.Collegerelevantsubjects);
            jobProfileIndex.ApprenticeshipRelevantSubjects = GetHtml(contentItem.Content.JobProfile.Apprenticeshiprelevantsubjects);
            jobProfileIndex.UniversityRelevantSubjects = GetHtml(contentItem.Content.JobProfile.Universityrelevantsubjects);
            jobProfileIndex.WYDDayToDayTasks = GetHtml((contentItem.Content.JobProfile.Daytodaytasks));
            jobProfileIndex.CareerPathAndProgression = GetHtml(contentItem.Content.JobProfile.Careerpathandprogression);

            jobProfileIndex.WorkingPattern = await Helper.GetContentItemNamesAsync(contentItem.Content.JobProfile.WorkingPattern, contentManager);
            jobProfileIndex.WorkingPatternDetails = await Helper.GetContentItemNamesAsync(contentItem.Content.JobProfile.WorkingPatternDetails, contentManager);
            jobProfileIndex.WorkingHoursDetails = await Helper.GetContentItemNamesAsync(contentItem.Content.JobProfile.WorkingHoursDetails, contentManager);
            jobProfileIndex.MinimumHours = string.IsNullOrEmpty(contentItem.Content.JobProfile.Minimumhours.Value.ToString()) ? default : (double)contentItem.Content.JobProfile.Minimumhours.Value;
            jobProfileIndex.MaximumHours = string.IsNullOrEmpty(contentItem.Content.JobProfile.Maximumhours.Value.ToString()) ? default : (double)contentItem.Content.JobProfile.Maximumhours.Value;

            return jobProfileIndex;

        }

        private static IEnumerable<string> GetJobCategoriesWithUrl(IEnumerable<ContentItem> contentItems) =>
            contentItems.Select(x => $"{x.As<TitlePart>().Title}|{x.As<PageLocationPart>().UrlName}");

        private static IEnumerable<string> GetJobCategoryUrls(IEnumerable<ContentItem> contentItems) =>
            contentItems.Select(x => $"{x.As<PageLocationPart>().UrlName}");

        private static string GetHtml(dynamic html)
        {
            string strValue = html is null ? string.Empty : html.Html.ToString();
            return strValue.HTMLToText();
        }
    }
}
