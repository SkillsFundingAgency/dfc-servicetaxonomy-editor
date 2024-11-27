using System.Data.Common;
using DFC.ServiceTaxonomy.CompUi.Dapper;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.Data;

namespace DFC.ServiceTaxonomy.CompUi.Repository
{
    public class DataService : IDataService
    {
        private readonly IDbConnectionAccessor _dbaAccessor;
        private readonly ILogger<DataService> _logger;
        private readonly IDapperWrapper _dapperWrapper;

        public DataService(
            IDbConnectionAccessor dbaAccessor,
            IDapperWrapper dapperWrapper,
            ILogger<DataService> logger)
        {
            _dapperWrapper = dapperWrapper;
            _dbaAccessor = dbaAccessor;
            _logger = logger;
        }

        public async Task<IEnumerable<RelatedContentData>> GetRelatedContentDataByContentItemIdAndPage(Processing processing)
        {
            IEnumerable<RelatedContentData>? resultList = new List<RelatedContentData>();

            var sql = $"SELECT DocumentId FROM RelatedContentItemIndex WITH (NOLOCK) " +
                      $"WHERE RelatedContentIds LIKE CONCAT('%', @contentItemId, '%') ";
            var documentIds = await ExecuteQuery<string>(sql, new { contentItemId = processing.ContentItemId });

            if (documentIds?.Count() > 0)
            {
                sql = $"SELECT ContentItemId, DisplayText, Author, ContentType, " +
                      $"JSON_VALUE(Content,'$.GraphSyncPart.Text') AS GraphSyncId, " +
                      $"JSON_VALUE(Content,'$.PageLocationPart.FullUrl') AS FullPageUrl " +
                      $"FROM Document AS D " +
                      $"INNER JOIN ContentItemIndex AS CII on CII.DocumentId = D.Id " +
                      $"WHERE D.Id IN @queryIds " +
                      $"AND Latest = 1 AND Published = 1";

                resultList = await ExecuteQuery<RelatedContentData>(sql, new { queryIds = documentIds });
            }

            return resultList;
        }

        /// <summary>
        /// Get the related content Ids for a particular item, where there is no link from the actual item back to the item being processed. 
        /// </summary>
        /// <param name="processing"></param>
        /// <returns>IEnumerable<RelatedContentData></returns>
        public async Task<IEnumerable<RelatedContentData>> GetRelatedContentDataByContentItemId(Processing processing)
        {
            IEnumerable<RelatedContentData>? resultList = new List<RelatedContentData>();

            try
            {
                var sql = $"SELECT RelatedContentIds FROM RelatedContentItemIndex WITH (NOLOCK) " +
                    $"WHERE ContentItemId = @contentItemId ";
                var relatedContentIds = await ExecuteQuery<string>(sql, new { contentItemId = processing.ContentItemId });

                if (relatedContentIds != null)
                {
                    var items = relatedContentIds?.FirstOrDefault()?.ToString().Split(',');
                    var documentIdList = new List<int>();
                    foreach (var itemId in items)
                    {
                        sql = $"SELECT DocumentId FROM RelatedContentItemIndex WITH (NOLOCK) " +
                          $"WHERE ContentItemId = @contentItemId " +
                          $"AND ContentType = 'JobProfile' ";
                        var documentIds = await ExecuteQuery<int>(sql, new { contentItemId =  itemId.Replace("'", "") });

                        documentIdList.AddRange(documentIds);
                    }

                    if (documentIdList?.Count() > 0)
                    {
                        sql = $"SELECT ContentItemId, DisplayText, Author, ContentType, " +
                              $"JSON_VALUE(Content,'$.GraphSyncPart.Text') AS GraphSyncId, " +
                              $"JSON_VALUE(Content,'$.PageLocationPart.FullUrl') AS FullPageUrl " +
                              $"FROM Document AS D " +
                              $"INNER JOIN ContentItemIndex AS CII on CII.DocumentId = D.Id " +
                              $"WHERE D.Id IN @queryIds " +
                              $"AND Latest = 1 AND Published = 1 ";

                        resultList = await ExecuteQuery<RelatedContentData>(sql, new { queryIds = documentIdList });
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.ToString());
            }

            return resultList;
        }

        private async Task<IEnumerable<T>?> ExecuteQuery<T>(string sql, object? param = null)
        {
            await using DbConnection? connection = _dbaAccessor.CreateConnection();
            return await _dapperWrapper.QueryAsync<T>(connection, sql, param);
        }
    }
}
