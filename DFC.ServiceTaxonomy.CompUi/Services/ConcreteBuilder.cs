using System.Data.Common;
using System.Diagnostics;
using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
using DFC.Common.SharedContent.Pkg.Netcore.Model.ContentItems;
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

namespace DFC.ServiceTaxonomy.CompUi.Services
{
    public class ConcreteBuilder : IBuilder
    {
        private readonly IDbConnectionAccessor _dbaAccessor;
        private readonly IDapperWrapper _dapperWrapper;
        private readonly ISharedContentRedisInterface _sharedContentRedisInterface;
        private readonly ILogger<ConcreteBuilder> _logger;

        private const string Published = "/PUBLISHED";
        private const string Draft = "/DRAFT";
        private const string AllPageBanners = "PageBanners/All";
        private const string Prefix = "<<contentapiprefix>>/sharedcontent";
        private const string DysacFilteringQuestion = "DYSAC/FilteringQuestions";
        private const string DysacQuestionSet = "DYSAC/QuestionSets";
        private const string DysacJobProfileCategories = "DYSAC/JobProfileCategories";
        private const string DysacJobProfileOverviews = "DYSAC/JobProfileOverviews";
        private const string DysacShortQuestion = "DYSAC/ShortQuestion";
        private const string DysacPersonalityTrait = "DYSAC/Traits";
        private const string JobProfileCategories = "JobProfiles/Categories";
        private const string TriageToolFilters = "TriageToolFilters/All";
        private const string TriagePages = "TriageToolPages";
        private const string SharedContent = "SharedContent";

        public ConcreteBuilder(IDbConnectionAccessor dbaAccessor,
            IDapperWrapper dapperWrapper,
            ISharedContentRedisInterface sharedContentRedisInterface,
            ILogger<ConcreteBuilder> logger)
        {
            _dapperWrapper = dapperWrapper;
            _dbaAccessor = dbaAccessor;
            _sharedContentRedisInterface = sharedContentRedisInterface;
            _logger = logger;
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
                    var nodeId = string.Concat("PageBanner", CheckLeadingChar(result.BannerParts.WebPageUrl));
                    success = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);
                    LogNodeInvalidation(processing, nodeId, success);
                }

                //Additionally delete all page banners.  
                success = await _sharedContentRedisInterface.InvalidateEntityAsync(AllPageBanners);
                LogNodeInvalidation(processing, AllPageBanners, success);
            }

            return success;
        }

        public async Task<bool> InvalidateSharedContentAsync(Processing processing)
        {
            var success = true;

            if (!string.IsNullOrEmpty(processing.Content))
            {
                var sharedContent = JsonConvert.DeserializeObject<SharedContent>(processing.Content);
                string nodeId = string.Concat(SharedContent,
                    CheckLeadingChar(sharedContent.SharedContentText.NodeId.Substring(Prefix.Length,
                    sharedContent.SharedContentText.NodeId.Length - Prefix.Length)));
                success = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);
                LogNodeInvalidation(processing, nodeId, success);
            }

            return success;
        }

        public async Task<bool> InvalidateDysacPersonalityFilteringQuestionAsync()
        {
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(DysacFilteringQuestion);
            _logger.LogInformation($"The following NodeId will be invalidated: {DysacFilteringQuestion}, success: {success}.");

            return success;
        }
        public async Task<bool> InvalidateDysacPersonalityQuestionSetAsync()
        {
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(DysacQuestionSet);

            return success;
        }

        public async Task<bool> InvalidateDysacPersonalityShortQuestionAsync()
        {
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(DysacShortQuestion);

            return success;
        }

        public async Task<bool> InvalidateDysacPersonalityTraitAsync()
        {
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(DysacPersonalityTrait);

            return success;
        }

        public async Task<bool> InvalidateTriageToolFiltersAsync(Processing processing)
        {
            var success = true;
            if (!string.IsNullOrEmpty(processing.Content))
            {
                success = await _sharedContentRedisInterface.InvalidateEntityAsync(TriageToolFilters);
                success = await _sharedContentRedisInterface.InvalidateEntityAsync(TriagePages);
                LogNodeInvalidation(processing, TriageToolFilters, success);
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

                    case nameof(PublishedContentTypes.PersonalityTrait):
                        success = await InvalidateDysacPersonalityTraitAsync();
                        break;
                    case nameof(PublishedContentTypes.PersonalityShortQuestion):
                        success = await InvalidateDysacPersonalityShortQuestionAsync();
                        break;
                    case nameof(PublishedContentTypes.JobProfileCategory):
                        success = await InvalidateJobProfileCategoriesAsync(item);
                        break;
                    default:
                        _logger.LogError($"{processing.EventType}. The content type {processing.ContentType} could not be matched.");
                        break;
                }
            }

            return success;
        }

        public async Task InvalidateAdditionalPageNodesAsync(Processing processing)
        {
            if (processing.EventType == ProcessingEvents.DraftSaved)
            {
                await _sharedContentRedisInterface.InvalidateEntityAsync($"PageLocation{Draft}");
                await _sharedContentRedisInterface.InvalidateEntityAsync($"pagesurl{Draft}");
            }
            else
            {
                await _sharedContentRedisInterface.InvalidateEntityAsync($"PageLocation{Published}");
                await _sharedContentRedisInterface.InvalidateEntityAsync($"pagesurl{Published}");
            }
        }

        public async Task<bool> InvalidatePageNodeAsync(string content, ProcessingEvents processingEvents)
        {
            if (!string.IsNullOrEmpty(content))
            {
                var pageLocation = JsonConvert.DeserializeObject<Page>(content);
                string? nodeId;

                if (processingEvents == ProcessingEvents.DraftSaved)
                {
                    nodeId = string.Concat(PublishedContentTypes.Page.ToString(), CheckLeadingChar(pageLocation.PageLocationParts.FullUrl), Draft);
                }
                else
                {
                    nodeId = string.Concat(PublishedContentTypes.Page.ToString(), CheckLeadingChar(pageLocation.PageLocationParts.FullUrl), Published);
                }

                var success = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);
                success = await _sharedContentRedisInterface.InvalidateEntityAsync(TriageToolFilters);
                success = await _sharedContentRedisInterface.InvalidateEntityAsync(TriagePages);
                LogNodeInvalidation(nameof(processingEvents), string.Empty, nameof(PublishedContentTypes.Page), nodeId, success);

                return success;
            }

            return true;
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

                        var categoryNode = string.Concat(PublishedContentTypes.JobProfile.ToString(), "s", CheckLeadingChar(currentNodeResult.PageLocationParts.FullUrl));
                        success = await _sharedContentRedisInterface.InvalidateEntityAsync(categoryNode);
                        LogNodeInvalidation(processing, categoryNode, success);

                        var nodeId = string.Concat(DysacJobProfileCategories, CheckLeadingChar(currentNodeResult.PageLocationParts.FullUrl));
                        success = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);

                        LogNodeInvalidation(processing, nodeId, success);
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

        public async Task<bool> InvalidateJobProfileCategoriesAsync(RelatedItems relatedItems)
        {
            var success = true;

            if (relatedItems != null)
            {
                IEnumerable<NodeItem>? contentId = await GetContentItem(relatedItems.ContentItemId, 0, 0);

                if (contentId != null)
                {
                    var currentNode = contentId.FirstOrDefault();
                    var currentNodeResult = JsonConvert.DeserializeObject<Page>(currentNode.Content);

                    var nodeId = string.Concat(PublishedContentTypes.JobProfile.ToString(), "s", CheckLeadingChar(currentNodeResult.PageLocationParts.FullUrl));
                    success = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);
                    LogNodeInvalidation(nameof(ProcessingEvents.Published), relatedItems.ContentItemId, nameof(PublishedContentTypes.JobProfileCategory), nodeId, success);

                    success = await _sharedContentRedisInterface.InvalidateEntityAsync(DysacJobProfileCategories);
                    LogNodeInvalidation(nameof(ProcessingEvents.Published), relatedItems.ContentItemId, nameof(PublishedContentTypes.JobProfileCategory), DysacJobProfileCategories, success);
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
            await ProcessJobProfileOverviewInvalidations(processing, processing.Content);
        }

        public async Task InvalidateDysacJobProfileOverviewRelatedContentItemsAsync(Processing processing)
        {
            var data = await GetContentItemsByLikeQueryAsync(nameof(PublishedContentTypes.JobProfile), processing.ContentItemId);

            if (data != null)
            {
                foreach (var item in data)
                {
                    await ProcessJobProfileOverviewInvalidations(processing, item.Content);
                }
            }
        }

        private async Task ProcessJobProfileOverviewInvalidations(Processing processing, string content)
        {
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(DysacJobProfileOverviews);
            LogNodeInvalidation(processing, DysacJobProfileOverviews, success);
        }

        public async Task<bool> InvalidateJobProfileCategoryAsync()
        {
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(JobProfileCategories);
            return success;
        }

        public async Task<bool> InvalidateJobProfileAsync(Processing processing)
        {
            var result = JsonConvert.DeserializeObject<Page>(processing.Content);
            var nodeId = string.Concat(PublishedContentTypes.JobProfile.ToString(), "s", CheckLeadingChar(result.PageLocationParts.FullUrl));
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);
            LogNodeInvalidation(processing, nodeId, success);
            return success;
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
        private void LogNodeInvalidation(Processing processing, string nodeId, bool status)
        {
            _logger.LogInformation($"Event Type: {processing.EventType}. " +
                $"Content Item Id: {processing.DocumentId}. " +
                $"Content Type: {processing.ContentType}.  " +
                $"The following NodeId will be invalidated: {nodeId}. " +
                $"Success: {status}.");
        }

        [DebuggerStepThrough]
        private void LogNodeInvalidation(string eventType, string documentId, string contentType, string nodeId, bool status)
        {
            _logger.LogInformation($"Event Type: {eventType}. " +
                $"Content Item Id: {documentId}. " +
                $"Content Type: {contentType}.  " +
                $"The following NodeId will be invalidated: {nodeId}. " +
                $"Success: {status}.");
        }
    }
}
