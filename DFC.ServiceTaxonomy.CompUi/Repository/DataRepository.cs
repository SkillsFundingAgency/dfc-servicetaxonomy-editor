using System.Data.Common;
//using DFC.ServiceTaxonomy.CompUi.Dapper;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Models;
using OrchardCore.Data;
//using Dapper;
using System.Text.RegularExpressions;
using System.Data;
using System.Collections.Generic;
using Dapper;
//using Dapper;

namespace DFC.ServiceTaxonomy.CompUi.Repository
{
    public class DataService : IDataService
    {
        private readonly IDbConnectionAccessor _dbaAccessor;
        //private readonly IDapperWrapper _dapperWrapper;

        private static readonly Regex regParameters = new Regex(@"@\w+", RegexOptions.Compiled);

        public DataService(
            IDbConnectionAccessor dbaAccessor
            //IDapperWrapper dapperWrapper
            )
        {
            //_dapperWrapper = dapperWrapper;
            _dbaAccessor = dbaAccessor;
        }

        public async Task<IEnumerable<RelatedContentData>> GetRelatedContentData(Processing processing)
        {
            IEnumerable<RelatedContentData>? resultList = [];
            //var documentIds = string.Empty;

            //DbParameter parameters = new DbParameter()
            //{
            //    ParameterName = "ContentItemId",

            //}

            //var connectionTest = _dbaAccessor.CreateConnection();
            //Object[] parameters = { processing.DocumentId }; 
            //using (DbCommand command = connectionTest.CreateCommand())
            //{
            //    command.CommandText = $"SELECT DocumentId FROM RelatedContentItemIndex WITH (NOLOCK) WHERE RelatedContentIds LIKE '%@ContentItemId%' AND ContentType = 'Page' ";
            //    command.CommandType = System.Data.CommandType.Text;

            //    if (parameters != null)
            //    {
            //        MatchCollection cmdParams = regParameters.Matches(command.CommandText);
            //        List<String> param = new List<String>();
            //        foreach (var el in cmdParams)
            //        {
            //            if (!param.Contains(el.ToString()))
            //            {
            //                param.Add(el.ToString());
            //            }
            //        }

            //    }

            //}

            //using (IDbConnection conn = Connection)
            //{
            //    string sQuery = "SELECT ID, FirstName, LastName, DateOfBirth FROM Employee WHERE ID = @ID";
            //    conn.Open();
            //    var result = await conn.QueryAsync<Employee>(sQuery, new { ID = id });
            //    return result.FirstOrDefault();
            //}

            var sql = $"SELECT DocumentId FROM RelatedContentItemIndex WITH (NOLOCK) WHERE RelatedContentIds LIKE ('%@ContentItemId%') AND ContentType = 'Page' ";
            //sql = $"SELECT DocumentId FROM RelatedContentItemIndex WITH (NOLOCK) WHERE RelatedContentIds LIKE '%{processing.ContentItemId}%' AND ContentType = 'Page' ";

            //sql = "SELECT DocumentId FROM RelatedContentItemIndex WITH (NOLOCK) WHERE DocumentId = @ContentItemId AND ContentType = 'Page' ";
            sql = "SELECT DocumentId FROM RelatedContentItemIndex WITH (NOLOCK) WHERE DocumentId = 8222 AND ContentType = 'Page' ";

            //try
            //{

            //IEnumerable<List<DocumentIds>>? documentIds = new List<List<DocumentIds>>();
            //List<DocumentIds>? documentIdsTest = new List<DocumentIds>();
            await using (var connection = _dbaAccessor.CreateConnection())
            {
                var itemToFind = processing.ContentItemId;

                connection.Open();
                //var documentIds = await connection.QueryAsync<IEnumerable<string>>(sql);
                //documentIds = await connection.QueryAsync<DocumentIds>(sql, new { ContentItemId = processing.DocumentId });
                //documentIds = await connection.QueryAsync<List<DocumentIds>>(sql, new { ContentItemId = itemToFind });
                var documentIds = await connection.QueryAsync<DocumentIds>(sql, new { ContentItemId = itemToFind });
                //documentIds = await connection.QuerySingleOrDefault<DocumentIds>(sql, new { ContentItemId = itemToFind });
                //var documentIdsTest = await connection.QueryAsync<DocumentIds>(sql, new { ContentItemId = itemToFind });
                //var documentIdsTest2 = await connection.QueryAsync<DocumentIds>(sql);
                //test = await connection.QueryAsync<DocumentIds>(sql); dsgxdfdg
                //var test1 = await connection.QueryAsync<string>(sql, new { ContentItemId = processing.DocumentId });

            }


            //}
            //catch (Exception exception)
            //{
            //    var test = true;
            //}

            //var documentIds = await ExecuteQuery<string>(sql);

            //if (documentIds?.Count() > 0)
            //{
            //var queryIds = string.Join(",", documentIds);

            var queryIds = "8222,8282,8283";

            //sql = $"SELECT ContentItemId, DisplayText, Author, ContentType, " +
            //    $"JSON_VALUE(Content,'$.GraphSyncPart.Text') AS GraphSyncId, " +
            //    $"JSON_VALUE(Content,'$.PageLocationPart.FullUrl') AS FullPageUrl " +
            //    $"FROM Document AS D " +
            //    $"INNER JOIN ContentItemIndex AS CII on CII.DocumentId = D.Id " +
            //    $"WHERE D.Id IN ({queryIds}) " +
            //    $"AND Latest = 1 AND Published = 1";

            sql = $"SELECT ContentItemId, DisplayText, Author, ContentType, " +
                $"JSON_VALUE(Content,'$.GraphSyncPart.Text') AS GraphSyncId, " +
                $"JSON_VALUE(Content,'$.PageLocationPart.FullUrl') AS FullPageUrl " +
                $"FROM Document AS D WITH (NOLOCK)  " +
                $"INNER JOIN ContentItemIndex AS CII WITH (NOLOCK) ON CII.DocumentId = D.Id " +
                $"WHERE D.Id IN (@QueryIds) " +
                $"AND Latest = 1 AND Published = 1";

            //resultList = await ExecuteQuery<RelatedContentData>(sql);

            try
            {
                await using (var connection = _dbaAccessor.CreateConnection())
                {
                    connection.Open();
                    var test = await connection.QueryAsync<RelatedContentData>(sql, new { QueryIds = queryIds });
                }
            }
            catch (Exception exception)
            {
                var test = true;
            }

            //}
            return resultList;
        }
    }


    //private async Task<IEnumerable<T>?> ExecuteQuery<T>(string sql)
    //{
    //    IEnumerable<T>? results;

    //    await using (DbConnection? connection = _dbaAccessor.CreateConnection())
    //    {
    //        results = await _dapperWrapper.QueryAsync<T>(connection, sql);
    //    }

    //    return results;
    //}
}


public class DocumentIds()
{
    public int Id
    {
        get; set;
    }
}

