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

        public async Task<IEnumerable<RelatedContentData>> GetRelatedContentDataByContentItemIdAndPage(
            Processing processing)
        {
            IEnumerable<RelatedContentData>? resultList = [];

            var sql = $"SELECT DocumentId FROM RelatedContentItemIndex WITH (NOLOCK) " +
                      $"WHERE RelatedContentIds LIKE CONCAT('%', @contentItemId, '%') " +
                      $"AND ContentType = 'Page' ";
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

        private async Task<IEnumerable<T>?> ExecuteQuery<T>(string sql, object? param = null)
        {
            await using DbConnection? connection = _dbaAccessor.CreateConnection();
            return await _dapperWrapper.QueryAsync<T>(connection, sql, param);
        }
    }
}
