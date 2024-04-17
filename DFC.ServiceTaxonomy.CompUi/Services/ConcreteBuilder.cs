using System.Data.Common;
using System.Diagnostics;
using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Dapper;
using DFC.ServiceTaxonomy.CompUi.Enums;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Model;
using DFC.ServiceTaxonomy.CompUi.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrchardCore.Data;
using Page = DFC.ServiceTaxonomy.CompUi.Models.Page;
using SharedContent = DFC.ServiceTaxonomy.CompUi.Models.SharedContent;
using DFC.Common.SharedContent.Pkg.Netcore.Constant;
using DFC.ServiceTaxonomy.CompUi.AppRegistry;

namespace DFC.ServiceTaxonomy.CompUi.Services
{
    public class ConcreteBuilder : IBuilder
    {
        private readonly IDbConnectionAccessor _dbaAccessor;
        private readonly IDapperWrapper _dapperWrapper;
        private readonly ISharedContentRedisInterface _sharedContentRedisInterface;
        private readonly ILogger<ConcreteBuilder> _logger;
        private readonly IPageLocationUpdater _pageLocationUpdater;


        public ConcreteBuilder(IDbConnectionAccessor dbaAccessor,
            IDapperWrapper dapperWrapper,
            ISharedContentRedisInterface sharedContentRedisInterface,
            ILogger<ConcreteBuilder> logger,
             IPageLocationUpdater pageLocationUpdater)
        {
            _dapperWrapper = dapperWrapper;
            _dbaAccessor = dbaAccessor;
            _sharedContentRedisInterface = sharedContentRedisInterface;
            _logger = logger;
            _pageLocationUpdater = pageLocationUpdater;
        }

        public async Task<IEnumerable<NodeItem>> GetDataAsync(Processing processing)
            => await GetDataAsync(processing.DocumentId, processing.Latest, processing.Published);

        public async Task<IEnumerable<RelatedItems>?> GetRelatedContentItemIdsAsync(Processing processing)
            => await GetRelatedContentItemIdsAsync(processing.DocumentId);

        public async Task<IEnumerable<ContentItems>?> GetContentItemsByLikeQueryAsync(string contentType, string queryIds)
            => await GetContentItemsByLikeAsync(contentType, queryIds);

        public async Task<bool> InvalidateBannerAsync(Processing processing) => await InvalidatePageBannerAsync(processing);

        public async Task<bool> InvalidatePageBannerAsync(Processing processing)
        {
            var success = true;

            if (!string.IsNullOrEmpty(processing.Content))
            {
                var data = await GetRelatedContentItemIdsForBannersAsync(processing.DocumentId);

                if (data == null)
                {
                    _logger.LogError($"Event Type: {processing.EventType}. Content Type: {processing.ContentType}. No data could be found for the following ContentItemId: {processing.DocumentId}.");
                    return false;
                }

                foreach (var item in data)
                {
                    var result = JsonConvert.DeserializeObject<PageBanners>(item.Content);
                    var cacheKey = string.Concat("PageBanner", CheckLeadingChar(result.BannerParts.WebPageUrl));
                    success = await _sharedContentRedisInterface.InvalidateEntityAsync(cacheKey, processing.FilterType);
                    LogCacheKeyInvalidation(processing, cacheKey, processing.FilterType, success);
                }

                //Additionally delete all page banners.  
                success = await _sharedContentRedisInterface.InvalidateEntityAsync(ApplicationKeys.AllPageBanners, processing.FilterType);
                LogCacheKeyInvalidation(processing, ApplicationKeys.AllPageBanners, processing.FilterType, success);
            }

            return success;
        }

        public async Task<bool> InvalidateSharedContentAsync(Processing processing)
        {
            var success = true;

            if (!string.IsNullOrEmpty(processing.Content))
            {
                var sharedContent = JsonConvert.DeserializeObject<SharedContent>(processing.Content);
                string cacheKey = string.Concat(ApplicationKeys.SharedContent,
                    CheckLeadingChar(sharedContent.SharedContentText.NodeId.Substring(ApplicationKeys.Prefix.Length,
                    sharedContent.SharedContentText.NodeId.Length - ApplicationKeys.Prefix.Length))); ;
                success = await _sharedContentRedisInterface.InvalidateEntityAsync(cacheKey, processing.FilterType);
                LogCacheKeyInvalidation(processing, cacheKey, processing.FilterType, success);
            }

            return success;
        }

        public async Task<bool> InvalidateDysacPersonalityFilteringQuestionAsync(Processing processing)
        {
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(ApplicationKeys.DysacFilteringQuestion, processing.FilterType);
            LogCacheKeyInvalidation(processing, ApplicationKeys.DysacFilteringQuestion, processing.FilterType, success);

            return success;
        }
        public async Task<bool> InvalidateDysacPersonalityQuestionSetAsync(Processing processing)
        {
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(ApplicationKeys.DysacQuestionSet, processing.FilterType);
            LogCacheKeyInvalidation(processing, ApplicationKeys.DysacQuestionSet, processing.FilterType, success);

            return success;
        }

        public async Task<bool> InvalidateDysacPersonalityShortQuestionAsync(Processing processing)
        {
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(ApplicationKeys.DysacShortQuestion, processing.FilterType);
            _logger.LogInformation($"The following cache key will be invalidated: {ApplicationKeys.DysacShortQuestion}, filter: {processing.FilterType}, success: {success}.");
            return success;
        }

        public async Task<bool> InvalidateDysacPersonalityTraitAsync(Processing processing)
        {
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(ApplicationKeys.DysacPersonalityTrait, processing.FilterType);
            _logger.LogInformation($"The following cache key will be invalidated: {ApplicationKeys.DysacPersonalityTrait}, filter: {processing.FilterType}, success: {success}.");

            return success;
        }

        public async Task<bool> InvalidateTriageToolFiltersAsync(Processing processing)
        {
            var success = true;
            if (!string.IsNullOrEmpty(processing.Content))
            {
                success = await _sharedContentRedisInterface.InvalidateEntityAsync(ApplicationKeys.TriageToolFilters, processing.FilterType);
                LogCacheKeyInvalidation(processing, ApplicationKeys.TriageToolFilters, processing.FilterType, success);
                success = await _sharedContentRedisInterface.InvalidateEntityAsync(ApplicationKeys.TriagePages, processing.FilterType);
                LogCacheKeyInvalidation(processing, ApplicationKeys.TriagePages, processing.FilterType, success);
            }
            return success;
        }

        public async Task<bool> InvalidateAdditionalContentItemIdsAsync(Processing processing, IEnumerable<RelatedItems> data)
        {
            var success = true;

            foreach (var item in data)
            {
                switch (item.ContentType)
                {
                    //Add additional related content if necessary
                    case nameof(ContentTypes.PersonalityTrait):
                        success = await InvalidateDysacPersonalityTraitAsync(processing);
                        break;
                    case nameof(ContentTypes.PersonalityShortQuestion):
                        success = await InvalidateDysacPersonalityShortQuestionAsync(processing);
                        break;
                    case nameof(ContentTypes.JobProfileCategory):
                        success = await InvalidateJobProfileCategoriesAsync(processing);
                        break;
                    default:
                        _logger.LogError($"{processing.EventType}. The content type {processing.ContentType} could not be matched. Content Item Id: {processing.ContentItemId}");
                        break;
                }
            }

            return success;
        }

        public async Task InvalidateAdditionalPageNodesAsync(Processing processing)
        {
            await _sharedContentRedisInterface.InvalidateEntityAsync(ApplicationKeys.PageLocationSuffix, processing.FilterType);
            await _sharedContentRedisInterface.InvalidateEntityAsync(ApplicationKeys.PagesUrlSuffix, processing.FilterType);
            await _sharedContentRedisInterface.InvalidateEntityAsync(ApplicationKeys.SitemapPagesAll, processing.FilterType);
        }

        public async Task InvalidatePageNodeAsync(string content, Processing processing)
        {
            if (!string.IsNullOrEmpty(content))
            {
                var result = JsonConvert.DeserializeObject<Page>(content);
                string? cacheKey;
                string? pageNodeID = string.Concat("PageApi", CheckLeadingChar(result.GraphSyncParts.Text.Substring(result.GraphSyncParts.Text.LastIndexOf('/') + 1)));
                string? NodeId = result.GraphSyncParts.Text.Substring(result.GraphSyncParts.Text.LastIndexOf('/') + 1);
                List<string> locations = new List<string>();

                if (result.PageLocationParts.FullUrl == "/home")
                {
                    locations.Add("/");
                }

                if (result.PageLocationParts.DefaultPageForLocation == true)
                {
                    locations.Add(result.PageLocationParts.FullUrl.Substring(result.PageLocationParts.FullUrl.LastIndexOf('/')));
                }
                locations.Add(result.PageLocationParts.FullUrl);

                if (result.PageLocationParts.RedirectLocations != null)
                {
                    List<string> redirectUrls = result.PageLocationParts.RedirectLocations.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();

                    foreach (var url in redirectUrls)
                    {
                        locations.Add(url);
                    }
                }

                cacheKey = string.Concat(ContentTypes.Page.ToString(), CheckLeadingChar(result.PageLocationParts.FullUrl));

                if (processing.EventType == ProcessingEvents.Removed)
                {
                    await _pageLocationUpdater.DeletePages(NodeId, processing.FilterType);
                }
                else
                {
                    await _pageLocationUpdater.UpdatePages(NodeId, locations, processing.FilterType);

                }
                await _sharedContentRedisInterface.InvalidateEntityAsync(cacheKey, processing.FilterType);
                await _sharedContentRedisInterface.InvalidateEntityAsync(ApplicationKeys.TriageToolFilters, processing.FilterType);
                await _sharedContentRedisInterface.InvalidateEntityAsync(ApplicationKeys.TriagePages, processing.FilterType);
                LogCacheKeyInvalidation(nameof(processing.EventType), string.Empty, nameof(ContentTypes.Page), cacheKey, processing.FilterType, true);
            }
        }

        public async Task<bool> InvalidateJobProfileCategoriesAsync(Processing processing)
        {
            var success = true;
            var jobProfileCategories = JsonConvert.DeserializeObject<JobProfileCategoriesContent>(processing.Content);

            if (jobProfileCategories != null)
            {
                foreach (var category in jobProfileCategories.JobProfile.JobProfileCategory.ContentItemId)
                {
                    IEnumerable<NodeItem>? contentId = await GetContentItem(category, processing.Latest, processing.Published);

                    if (contentId != null)
                    {
                        var currentNode = contentId.FirstOrDefault();
                        var currentNodeResult = JsonConvert.DeserializeObject<Page>(currentNode.Content);

                        var categoryNode = string.Concat(ContentTypes.JobProfile.ToString(), "s", CheckLeadingChar(currentNodeResult.PageLocationParts.FullUrl));
                        success = await _sharedContentRedisInterface.InvalidateEntityAsync(categoryNode, processing.FilterType);
                        LogCacheKeyInvalidation(processing, categoryNode, processing.FilterType, success);

                        var cacheKey = string.Concat(ApplicationKeys.DysacJobProfileCategories, CheckLeadingChar(currentNodeResult.PageLocationParts.FullUrl));
                        success = await _sharedContentRedisInterface.InvalidateEntityAsync(cacheKey, processing.FilterType);

                        LogCacheKeyInvalidation(processing, cacheKey, processing.FilterType, success);

                        cacheKey = string.Concat(ApplicationKeys.ExploreCareersJobProfileCategories, CheckLeadingChar(currentNodeResult.PageLocationParts.FullUrl));
                        success = await _sharedContentRedisInterface.InvalidateEntityAsync(cacheKey, processing.FilterType);

                        LogCacheKeyInvalidation(processing, cacheKey, processing.FilterType, success);
                    }
                }
                return success;
            }
            else
            {
                _logger.LogError($"Event Type: {processing.EventType}. Content Type: {processing.ContentType}. No data could be found.");
                return false;
            }
        }

        public async Task<bool> InvalidateJobProfileCategoriesAsync(RelatedItems relatedItems, Processing processing)
        {
            var success = true;

            if (relatedItems != null)
            {
                IEnumerable<NodeItem>? contentId = await GetContentItem(relatedItems.ContentItemId, 0, 0);

                if (contentId != null)
                {
                    var currentNode = contentId.FirstOrDefault();
                    var currentNodeResult = JsonConvert.DeserializeObject<Page>(currentNode.Content);

                    var cacheKey = string.Concat(ContentTypes.JobProfile.ToString(), "s", CheckLeadingChar(currentNodeResult.PageLocationParts.FullUrl));
                    success = await _sharedContentRedisInterface.InvalidateEntityAsync(cacheKey, processing.FilterType);
                    LogCacheKeyInvalidation(nameof(ProcessingEvents.Published), relatedItems.ContentItemId,
                        nameof(ContentTypes.JobProfileCategory), cacheKey, processing.FilterType, success);

                    success = await _sharedContentRedisInterface.InvalidateEntityAsync(ApplicationKeys.DysacJobProfileCategories, processing.FilterType);
                    LogCacheKeyInvalidation(nameof(ProcessingEvents.Published),
                        relatedItems.ContentItemId, nameof(ContentTypes.JobProfileCategory), ApplicationKeys.DysacJobProfileCategories, processing.FilterType, success);

                    success = await _sharedContentRedisInterface.InvalidateEntityAsync(ApplicationKeys.ExploreCareersJobProfileCategories, processing.FilterType);
                    LogCacheKeyInvalidation(nameof(ProcessingEvents.Published),
                        relatedItems.ContentItemId, nameof(ContentTypes.JobProfileCategory), ApplicationKeys.ExploreCareersJobProfileCategories, processing.FilterType, success);
                }
                return success;
            }
            else
            {
                return false;
            }
        }

        public async Task InvalidateDysacJobProfileOverviewAsync(Processing processing)
        {
            await ProcessJobProfileOverviewInvalidations(processing);
        }

        public async Task InvalidateDysacJobProfileOverviewRelatedContentItemsAsync(Processing processing)
        {
            var data = await GetContentItemsByLikeQueryAsync(nameof(ContentTypes.JobProfile), processing.ContentItemId);

            if (data != null)
            {
                foreach (var item in data)
                {
                    await ProcessJobProfileOverviewInvalidations(processing);
                }
            }
        }

        private async Task ProcessJobProfileOverviewInvalidations(Processing processing)
        {
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(ApplicationKeys.DysacJobProfileOverviews, processing.FilterType);
            LogCacheKeyInvalidation(processing, ApplicationKeys.DysacJobProfileOverviews, processing.FilterType, success);
        }

        public async Task<bool> InvalidateJobProfileCategoryAsync(Processing processing)
        {
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(ApplicationKeys.DysacJobProfileCategories, processing.FilterType);
            success = await _sharedContentRedisInterface.InvalidateEntityAsync(ApplicationKeys.ExploreCareersJobProfileCategories, processing.FilterType);
            return success;
        }

        public async Task InvalidateJobProfileAsync(Processing processing)
        {
            var result = JsonConvert.DeserializeObject<Page>(processing.Content);
            var cacheKey = string.Concat(ApplicationKeys.JobProfileSuffix, CheckLeadingChar(result.PageLocationParts.FullUrl));
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(cacheKey, processing.FilterType);
            LogCacheKeyInvalidation(processing, cacheKey, processing.FilterType, success);
        }

        public async Task InvalidateJobProfileOverviewAsync(Processing processing)
        {
            var result = JsonConvert.DeserializeObject<Page>(processing.Content);
            var cacheKey = string.Concat(ApplicationKeys.JobProfilesOverview, CheckLeadingChar(result.PageLocationParts.FullUrl));
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(cacheKey, processing.FilterType);
            LogCacheKeyInvalidation(processing, cacheKey, processing.FilterType, success);
        }

        public async Task InvalidateJobProfileSkillsAsync(Processing processing)
        {
            var result = JsonConvert.DeserializeObject<Page>(processing.Content);
            var cacheKey = string.Concat(ApplicationKeys.JobProfileSkillsSuffix, CheckLeadingChar(result.PageLocationParts.FullUrl));
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(cacheKey, processing.FilterType);
            LogCacheKeyInvalidation(processing, cacheKey, processing.FilterType, success);
        }

        public async Task InvalidateJobProfileRelatedCareersAsync(Processing processing)
        {
            var result = JsonConvert.DeserializeObject<Page>(processing.Content);
            var cacheKey = string.Concat(ApplicationKeys.JobProfileRelatedCareersPrefix, CheckLeadingChar(result.PageLocationParts.FullUrl));
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(cacheKey, processing.FilterType);
            LogCacheKeyInvalidation(processing, cacheKey, processing.FilterType, success);
        }
        
        public async Task InvalidateJobProfileHowToBecomeAsync(Processing processing)
        {
            var result = JsonConvert.DeserializeObject<Page>(processing.Content);
            var cacheKey = string.Concat(ApplicationKeys.JobProfileHowToBecome, CheckLeadingChar(result.PageLocationParts.FullUrl));
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(cacheKey, processing.FilterType);
            LogCacheKeyInvalidation(processing, cacheKey, processing.FilterType, success);
        }

        public async Task InvalidateJobProfileWhatYoullDoAsync(Processing processing)
        {
            var result = JsonConvert.DeserializeObject<Page>(processing.Content);
            var cacheKey = string.Concat(ApplicationKeys.JobProfileWhatYoullDo, CheckLeadingChar(result.PageLocationParts.FullUrl));
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(cacheKey, processing.FilterType);
            LogCacheKeyInvalidation(processing, cacheKey, processing.FilterType, success);
        }

        public async Task InvalidateJobProfileVideoAsync(Processing processing)
        {
            var result = JsonConvert.DeserializeObject<Page>(processing.Content);
            var cacheKey = string.Concat(ApplicationKeys.JobProfileVideoPrefix, CheckLeadingChar(result.PageLocationParts.FullUrl));
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(cacheKey, processing.FilterType);
            LogCacheKeyInvalidation(processing, cacheKey, processing.FilterType, success);
        }

        public async Task InvalidateJobProfileCurrentOpportunitiesAllAsync(Processing processing)
        {
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(ApplicationKeys.JobProfileCurrentOpportunitiesAllJobProfiles, processing.FilterType);
            LogCacheKeyInvalidation(processing, ApplicationKeys.JobProfileCurrentOpportunitiesAllJobProfiles, processing.FilterType, success);
        }

        private async Task<IEnumerable<NodeItem>?> GetDataAsync(int contentItemId, int latest, int published)
        {
            var sql = $"SELECT DISTINCT GSPI.NodeId, D.Content " +
                    $"FROM Document D WITH (NOLOCK) " +
                    $"JOIN ContentItemIndex CII WITH (NOLOCK) ON D.Id = CII.DocumentId " +
                    $"JOIN GraphSyncPartIndex GSPI  WITH (NOLOCK) ON CII.DocumentId = GSPI.DocumentId " +
                    $"WHERE D.Id = {contentItemId} " +
                    $"AND CII.Latest = {latest} AND CII.Published = {published} ";

            return await ExecuteQuery<NodeItem>(sql);
        }

        private async Task<IEnumerable<ContentItems>?> GetRelatedContentItemIdsForBannersAsync(int documentId)
        {
            try
            {
                var sql = $"SELECT ContentItemId FROM RelatedContentItemIndex WITH (NOLOCK) WHERE DocumentId = {documentId} ";
                var relatedContentIds = await ExecuteQuery<string>(sql);

                if (relatedContentIds?.Count() > 0)
                {
                    var queryIds = string.Empty;
                    foreach (var item in relatedContentIds)
                    {
                        queryIds += item;
                    }

                    var invalidationList = await GetContentItemsByLikeAsync("Pagebanner", queryIds);

                    return invalidationList;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error occurred while retrieveing data for document Id {documentId}.  Exception: {exception}.");
            }

            return null;
        }

        private async Task<IEnumerable<ContentItems>?> GetContentItemsByLikeAsync(string contentType, string queryIds)
        {
            var sql = $"SELECT DISTINCT Content, ContentType FROM ContentItemIndex CII WITH(NOLOCK) " +
                                    $"JOIN Document D WITH(NOLOCK) ON D.Id = CII.DocumentId " +
                                    $"WHERE CII.Published = 1 " +
                                    $"AND CII.Latest = 1  " +
                                    $"AND CII.ContentType = '{contentType}' " +
                                    $"AND D.Content LIKE '%{queryIds}%' ";

            var invalidationList = await ExecuteQuery<ContentItems>(sql);

            return invalidationList;
        }

        private async Task<IEnumerable<RelatedItems>?> GetRelatedContentItemIdsAsync(int documentId)
        {
            try
            {
                //Ideally use the following query to get all the data in one go, although it will likely "break" SQL Lite:
                //SELECT* FROM ContentItemIndex AS CII WITH(NOLOCK) WHERE ContentItemId IN
                //(
                //    SELECT VALUE RelatedContentId FROM RelatedContentItemsIndex WITH(NOLOCK)
                //    CROSS APPLY STRING_SPLIT(RelatedContentIds, ',')
                //    WHERE DocumentId = 1963
                //)
                //AND CII.Published = 1 AND CII.Latest = 1

                var sql = $"SELECT RelatedContentIds FROM RelatedContentItemIndex WITH (NOLOCK) WHERE DocumentId = {documentId} ";
                var relatedContentIds = await ExecuteQuery<string>(sql);

                if (relatedContentIds?.Count() > 0)
                {
                    var queryIds = string.Empty;

                    //Temp work around.  There must be a way of just returning a simple string from the query.  
                    foreach (var item in relatedContentIds)
                    {
                        queryIds += item;
                    }

                    sql = $"SELECT ContentItemId, ContentType FROM ContentItemIndex AS CII WITH (NOLOCK) WHERE ContentItemId IN ({queryIds}) AND CII.Published = 1 AND CII.Latest = 1 ";
                    var invalidationList = await ExecuteQuery<RelatedItems>(sql);

                    return invalidationList;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            return null;
        }

        private async Task<IEnumerable<T>?> ExecuteQuery<T>(string sql)
        {
            IEnumerable<T>? results;

            await using (DbConnection? connection = _dbaAccessor.CreateConnection())
            {
                results = await _dapperWrapper.QueryAsync<T>(connection, sql);
            }

            return results;
        }

        private async Task<IEnumerable<NodeItem>?> GetContentItem(string contentItem, int latest, int published)
        {
            var sql = $"SELECT DISTINCT D.Content " +
                    $"FROM Document D WITH (NOLOCK) " +
                    $"JOIN ContentItemIndex CII WITH (NOLOCK) ON D.Id = CII.DocumentId " +
                    $"WHERE CII.ContentItemId = '{contentItem}' ";

            return await ExecuteQuery<NodeItem>(sql);
        }

        [DebuggerStepThrough]
        private string CheckLeadingChar(string input)
        {
            if (input.FirstOrDefault() != '/')
            {
                input = '/' + input;
            }

            return input;
        }

        [DebuggerStepThrough]
        private void LogCacheKeyInvalidation(Processing processing, string cacheKey, string filter, bool status)
        {
            _logger.LogInformation($"Event Type: {processing.EventType}. " +
                $"Content Item Id: {processing.DocumentId}. " +
                $"Content Type: {processing.ContentType}.  " +
                $"The following Cache Key will be invalidated: {cacheKey}. Filter: {filter}." +
                $"Success: {status}.");
        }

        [DebuggerStepThrough]
        private void LogCacheKeyInvalidation(string eventType, string documentId, string contentType, string cacheKey, string filter, bool status)
        {
            _logger.LogInformation($"Event Type: {eventType}. " +
                $"Content Item Id: {documentId}. " +
                $"Content Type: {contentType}.  " +
                $"The following Cache Key will be invalidated: {cacheKey}. Filter: {filter}." +
                $"Success: {status}.");
        }
    }
}
