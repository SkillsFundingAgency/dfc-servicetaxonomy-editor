using System.Diagnostics;
using DFC.Common.SharedContent.Pkg.Netcore.Constant;
using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
using DFC.Common.SharedContent.Pkg.Netcore.Model.Response;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.CompUi.BackgroundTask.Activity
{
    public class JobProfileCacheRefresh : IJobProfileCacheRefresh
    {
        private readonly ISharedContentRedisInterface sharedContentRedisInterface;
        private readonly ILogger<JobProfileCacheRefresh> _logger;

        public JobProfileCacheRefresh(ISharedContentRedisInterface sharedContentRedisInterface, ILogger<JobProfileCacheRefresh> logger)
        {
            this._logger = logger;
            this.sharedContentRedisInterface = sharedContentRedisInterface;
        }

        public async Task RefreshAllJobProfileContent(Processing processing)
        {
            try
            {
                var current = JsonConvert.DeserializeObject<ContentItem>(processing.CurrentContent);
                var fullUrl = current?.PageLocationParts?.FullUrl;
                var filter = processing.FilterType?.ToString() ?? "PUBLISHED";

                if (string.IsNullOrEmpty(fullUrl))
                {
                    _logger.LogError("Error occurred while retrieveing data for document Id {DocumentId}.  Content Type: {ContentType}. Page content could not be retrieved. No Job Profile data will be refreshed.",
                        processing.DocumentId,
                        processing.ContentType);
                }
                else
                {
                    await GetDataWithExpiryAsync<JobProfileCurrentOpportunitiesResponse>(processing, ApplicationKeys.JobProfileCurrentOpportunitiesAllJobProfiles, filter);
                    await GetDataWithExpiryAsync<RelatedCareersResponse>(processing, string.Concat(ApplicationKeys.JobProfileRelatedCareersPrefix, fullUrl), filter);
                    await GetDataWithExpiryAsync<JobProfileHowToBecomeResponse>(processing, string.Concat(ApplicationKeys.JobProfileHowToBecome, fullUrl), filter);
                    await GetDataWithExpiryAsync<JobProfilesOverviewResponse>(processing, string.Concat(ApplicationKeys.JobProfileOverview, fullUrl), filter);
                    await GetDataWithExpiryAsync<JobProfileVideoResponse>(processing, string.Concat(ApplicationKeys.JobProfileVideoPrefix, fullUrl), filter);
                    await GetDataWithExpiryAsync<JobProfileCurrentOpportunitiesGetbyUrlReponse>(processing, ApplicationKeys.JobProfileCurrentOpportunitiesAllJobProfiles, filter);
                    await GetDataWithExpiryAsync<JobProfileWhatYoullDoResponse>(processing, string.Concat(ApplicationKeys.JobProfileWhatYoullDo, fullUrl), filter);
                    await GetDataWithExpiryAsync<JobProfileCareerPathAndProgressionResponse>(processing, string.Concat(ApplicationKeys.JobProfileCareerPath, fullUrl), filter);
                    await GetDataWithExpiryAsync<JobProfileSkillsResponse>(processing, string.Concat(ApplicationKeys.JobProfileSkillsSuffix, fullUrl), filter);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error occurred while refreshing Job Profile data. Exception Message: {Message}. Stack Trace: {StackTrace}.", exception.Message, exception.StackTrace);
            }
        }

        private async Task GetDataWithExpiryAsync<T>(Processing processing, string cacheKey, string filter)
        {
            try
            {
                await sharedContentRedisInterface.GetDataAsyncWithExpiry<T>(cacheKey, filter);
                LogCacheKeyRefresh(processing, cacheKey, filter);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error occurred while refreshing Job Profile data. Exception Message: {Message}. Stack Trace: {StackTrace}.", exception.Message, exception.StackTrace);
            }
        }

        [DebuggerStepThrough]
        private void LogCacheKeyRefresh(Processing processing, string cacheKey, string filter)
        {
            _logger.LogInformation("Event Type: {EventType}.\nContent Item Id: {DocumentId}.\nContent Type: {ContentType}.\nThe following Cache Key will be refreshed: {cacheKey}. Filter: {filter}.",
                processing.EventType,
                processing.DocumentId,
                processing.ContentType,
                cacheKey,
                filter);
        }
    }
}
