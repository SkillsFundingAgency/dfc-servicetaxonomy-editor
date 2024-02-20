using System.Data.Common;
using System.Diagnostics;
using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
using DFC.Common.SharedContent.Pkg.Netcore.Model.ContentItems.PageBanner;
using DFC.ServiceTaxonomy.CompUi.Dapper;
using DFC.ServiceTaxonomy.CompUi.Enums;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Model;
using DFC.ServiceTaxonomy.CompUi.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrchardCore.Data;
using static NpgsqlTypes.NpgsqlTsQuery;
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
        private const string DysacFilteringQuestion = "DYSAC/FilteringQuestion";
        private const string DysacQuestionSet = "DYSAC/QuestionSet";
        private const string DysacShortQuestion = "DYSAC/ShortQuestion";  //TBC with the dev
        private const string DysacPersonalityTrait = "DYSAC/PersonalityTrait";  //TBC with the dev
        private const string JobProfileCategories = "JobProfiles/Categories";
        private const string JobProfileOverview = "JobProfileOverview";

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
        {
            var results = await GetDataAsync(processing.ContentItemId, processing.Latest, processing.Published);
            return results;
        }

        public async Task<IEnumerable<RelatedItems>?> GetRelatedContentItemIdsAsync(Processing processing)
        {
            var results = await GetRelatedContentItemIdsAsync(processing.ContentItemId);
            return results;
        }

        public Task<string> GetSharedContentNodeIdAsync(Processing processing, IEnumerable<NodeItem> data)
        {
            //Get the node here - se
            Console.WriteLine("GetSharedContentNodeIdAsync");
            //throw new NotImplementedException();
            var test = "test";

            return Task.FromResult(test);
        }

        public async Task<bool> InvalidateBannerAsync(Processing processing)
        {
            var succcess = await InvalidatePageBannerAsync(processing);
            return succcess;
        }

        public async Task<bool> InvalidatePageBannerAsync(Processing processing)
        {
            var success = true;

            if (!string.IsNullOrEmpty(processing.Content))
            {
                var result = JsonConvert.DeserializeObject<PageBanners>(processing.Content);

                //TODO - check this for multiple banners.  Should be okay, however check for this anyway. 
                var nodeId = string.Concat("PageBanner", CheckLeadingChar(result.BannerParts.WebPageUrl));
                success = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);
                _logger.LogInformation($"{processing.EventType}. The following NodeId will be invalidated: {nodeId}, success: {success}.");

                //Additionally delete all page banners.  
                success = await _sharedContentRedisInterface.InvalidateEntityAsync(AllPageBanners);
                _logger.LogInformation($"{processing.EventType}. The following NodeId will be invalidated: {AllPageBanners}, success: {success}");
            }

            return success;
        }

        public async Task<bool> InvalidateSharedContentAsync(Processing processing)
        {
            var success = true;

            if (!string.IsNullOrEmpty(processing.Content))
            {
                var sharedContent = JsonConvert.DeserializeObject<SharedContent>(processing.Content);
                string nodeId = string.Concat(PublishedContentTypes.SharedContent.ToString(),
                    CheckLeadingChar(sharedContent.SharedContentText.NodeId.Substring(Prefix.Length,
                    sharedContent.SharedContentText.NodeId.Length - Prefix.Length)));
                success = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);
            }

            return success;
        }

        public async Task<bool> InvalidatePersonalityFilteringQuestionAsync(Processing processing)
        {
            var success = true;

            if (!string.IsNullOrEmpty(processing.Content))
            {
                success = await _sharedContentRedisInterface.InvalidateEntityAsync(DysacFilteringQuestion);
                _logger.LogInformation($"{processing.EventType}. The following NodeId will be invalidated: {DysacFilteringQuestion}, success: {success}.");
            }

            return success;
        }
        public async Task<bool> InvalidatePersonalityQuestionSetAsync(Processing processing)
        {
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(DysacQuestionSet);

            return success;
        }

        public async Task<bool> InvalidatePersonalityShortQuestionAsync(Processing processing)
        {
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(DysacShortQuestion);

            return success;
        }

        public async Task<bool> InvalidatePersonalityTraitAsync(Processing processing)
        {
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(DysacPersonalityTrait);

            return success;
        }

        public async Task<bool> InvalidateAdditionalContentItemIdsAsync(Processing processing, IEnumerable<RelatedItems> data)
        {
            var success = true;

            foreach (var item in data)
            {
                switch (item.ContentType)
                {
                    case nameof(PublishedContentTypes.PersonalityTrait):
                        success = await InvalidatePersonalityTraitAsync(processing);
                        break;
                    case nameof(PublishedContentTypes.PersonalityShortQuestion):
                        success = await InvalidatePersonalityShortQuestionAsync(processing);
                        break;
                    default:
                        _logger.LogInformation($"{processing.EventType}. The content type {processing.ContentType} could not be matched.");
                        break;
                }
            }

            /*       
                TriageToolFilter - James to code
                TriageToolOption - James to code
                PersonalityFilteringQuestion - do we need to just call the above?
                PersonalityQuestionSet - do we need to just call the above?
                PersonalityShortQuestion,
                PersonalityTrait,
            */


            //For each contentId we have in data we need to invalidate this?
            //What's a nice way of doing this?
            //Have a switch statement calling the relevant method in here?  Something else?
            //This method is for
            //SOCCode,
            //SOCSkillsMatrix,
            //DynamicTitlePrefix

            //**********************
            //Confirm node format
            //From Zhaoming
            //PersonalityFilteringQuestion: DYSAC/FilteringQuestion
            //PersonalityQuestionSet: DYSAC/QuestionSet
            //**********************

            //success = await _sharedContentRedisInterface.InvalidateEntityAsync($"pagesurl{Published}");

            return success;
        }

        public async Task<bool> InvalidateAdditionalPageNodesAsync(Processing processing)
        {
            var success = true;
            if (processing.EventType == ProcessingEvents.DraftSaved)
            {
                success = await _sharedContentRedisInterface.InvalidateEntityAsync($"PageLocation{Draft}");
                success = await _sharedContentRedisInterface.InvalidateEntityAsync($"pagesurl{Draft}");
            }
            else
            {
                success = await _sharedContentRedisInterface.InvalidateEntityAsync($"PageLocation{Published}");
                success = await _sharedContentRedisInterface.InvalidateEntityAsync($"pagesurl{Published}");
            }
            return success;
        }

        public async Task<bool> InvalidatePageNodeAsync(Processing processing)
        {
            if (!string.IsNullOrEmpty(processing.Content))
            {
                var pageLocation = JsonConvert.DeserializeObject<Page>(processing.Content);
                var nodeId = string.Empty;

                if (processing.EventType == ProcessingEvents.DraftSaved)
                {
                    nodeId = string.Concat(PublishedContentTypes.Page.ToString(), CheckLeadingChar(pageLocation.PageLocationParts.FullUrl), Draft);
                }
                else
                {
                    nodeId = string.Concat(PublishedContentTypes.Page.ToString(), CheckLeadingChar(pageLocation.PageLocationParts.FullUrl), Published);
                }

                return await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);
            }

            return true;
        }

        //public Task<bool> InvalidateJobProfileCategoryAsync(Processing processing)
        //{
        //    var success = true;

        //    if (!string.IsNullOrEmpty(processing.Content))
        //    {
        //        //Need to get all job profile categories 
        //    }

        //    return success;  
        //}

        //public Task<bool> InvalidateNodeAsync(Processing processing)
        //{
        //    //Call the NuGet package invalidate method here.  
        //    //throw new NotImplementedException();
        //    return Task.FromResult(test);
        //    return Task.FromResult(test);
        //}

        public async Task<bool> InvalidateJobProfileCategories(Processing processing)
        {
            //var result = JsonConvert.DeserializeObject<Page>(processing.Content);
            //var nodeId = string.Concat(PublishedContentTypes.JobProfile.ToString(), "s", CheckLeadingChar(result.PageLocationParts.FullUrl));
            //var success = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);

            var success = true; 
            var jobProfileCategories = JsonConvert.DeserializeObject<JobProfileCategoriesContent>(processing.Content);

            foreach (var category in jobProfileCategories.JobProfile.JobProfileCategory.ContentItemId)
            {
                IEnumerable<NodeItem>? contentId = await GetContentItem(category, processing.Latest, processing.Published);

                if (contentId != null)
                {
                    var currentNode = contentId.FirstOrDefault();
                    var currentNodeResult = JsonConvert.DeserializeObject<Page>(currentNode.Content);
                    var categoryNode = string.Concat(PublishedContentTypes.JobProfile.ToString(), "s", CheckLeadingChar(currentNodeResult.PageLocationParts.FullUrl));
                    success = await _sharedContentRedisInterface.InvalidateEntityAsync(categoryNode);
                }
            }

            return success;
        }

        public async Task<bool> InvalidateJobProfileOverview(Processing processing)
        {
            //We need to invalidate the following:
            //"JobProfileOverview/e-learning developer"
            //"JobProfileOverview/climate scientist"

            var jobProfile = JsonConvert.DeserializeObject<JobProfileCategoriesContent>(processing.Content);
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(string.Format(JobProfileOverview, CheckLeadingChar(jobProfile.TitlePart.Title)));
            return true;
        }

        public async Task<bool> InvalidateJobProfileCategory()
        {
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(JobProfileCategories);
            return success;
        }
        public async Task<bool> InvalidateJobProfile(Processing processing)
        {
            var result = JsonConvert.DeserializeObject<Page>(processing.Content);
            var nodeId = string.Concat(PublishedContentTypes.JobProfile.ToString(), "s", CheckLeadingChar(result.PageLocationParts.FullUrl));
            var success = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);
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

        private async Task<IEnumerable<RelatedItems>?> GetRelatedContentItemIdsAsync(int documentId)
        {
            try
            {
                //Ideally use the following query to get all the data in one go, although it will likely "break" SQL Lite:
                //SELECT* FROM ContentItemIndex AS CII WITH(NOLOCK) WHERE ContentItemId IN
                //(
                //    SELECT VALUE RelatedContentId FROM RelatedContentItemIndex WITH(NOLOCK)
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

        //private async Task<string> ExecuteQuery(string sql)
        //{
        //    string results;

        //    await using (DbConnection? connection = _dbaAccessor.CreateConnection())
        //    {
        //        results = await _dapperWrapper.QueryAsync(connection, sql);
        //    }

        //    return results;
        //}

        //public string FormatPageBannerNodeId(string content)
        //{
        //    var result = JsonConvert.DeserializeObject<PageBanners>(content);
        //    return string.Concat("PageBanner", CheckLeadingChar(result.BannerParts.WebPageUrl));
        //}

        [DebuggerStepThrough]
        private string CheckLeadingChar(string input)
        {
            if (input.FirstOrDefault() != '/')
            {
                input = '/' + input;
            }

            return input;
        }
    }
}
