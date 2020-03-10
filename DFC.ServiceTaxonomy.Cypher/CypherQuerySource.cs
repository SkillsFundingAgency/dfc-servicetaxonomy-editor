using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Cypher.Extensions;
using DFC.ServiceTaxonomy.Cypher.Helpers;
using DFC.ServiceTaxonomy.Cypher.Models.ResultModels;
using Newtonsoft.Json;
using OrchardCore.Queries;

namespace DFC.ServiceTaxonomy.Cypher
{
    public class CypherQuerySource : IQuerySource
    {
        private readonly INeo4JHelper neo4JHelper;

        public string Name => "Cypher";

        public CypherQuerySource(INeo4JHelper neo4JHelper)
        {
            this.neo4JHelper = neo4JHelper;
        }

        public Query Create()
        {
            var result = new Models.CypherQuery() { };

            return result;
        }

        public async Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
        {
            var cypherQuery = query as Models.CypherQuery;

            if (cypherQuery == null)
            {
                return null;
            }

            var parameterValues = BuildParameters(cypherQuery.Parameters, parameters);
            var result = new Models.CypherQueryResult();
            var cypherResult = await neo4JHelper.ExecuteCypherQueryInNeo4JAsync(cypherQuery.Template, parameterValues);
            var collections = cypherResult as Dictionary<string, object>;

            if (collections != null)
            {
                result.Items = TransformResults(cypherQuery.ResultModelType, collections);
            }

            return result;
        }

        private IDictionary<string, object> BuildParameters(string cypherQueryParameters, IDictionary<string, object> parameters)
        {
            IDictionary<string, object> parameterValues = null;

            if (!string.IsNullOrWhiteSpace(cypherQueryParameters))
            {
                parameterValues = JsonConvert.DeserializeObject<IDictionary<string, object>>(cypherQueryParameters);

                if (parameterValues != null)
                {
                    if (parameters != null)
                    {
                        foreach (var key in parameters.Keys)
                        {
                            if (parameterValues.ContainsKey(key))
                            {
                                parameterValues[key] = parameters[key];
                            }
                        }
                    }

                    for (var i = 0; i < parameterValues.Keys.Count; i++)
                    {
                        var key = parameterValues.Keys.ElementAt(i);
                        var value = parameterValues[key] as Newtonsoft.Json.Linq.JArray;

                        if (value != null)
                        {
                            parameterValues[key] = value.ToObject<string[]>();
                        }
                    }
                }
            }

            return parameterValues;
        }

        private IEnumerable<object> TransformResults(string resultModelType, Dictionary<string, object>  collections)
        {
            IEnumerable<object> result = null;
            var genericTypeName = $"{typeof(Startup).Namespace}.Models.ResultModels.{resultModelType}";
            var genericType = Type.GetType(genericTypeName);

            if (genericType != null)
            {
                var methodInfo = GetType().GetMethod(nameof(TransformResultCollections), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(genericType);

                result = (IEnumerable<object>)methodInfo.Invoke(this, new object[] { collections });
            }

            return result;
        }

        private List<TModel> TransformResultCollections<TModel>(Dictionary<string, object> collections)
            where TModel : class, IQueryResultModel, new()
        {
            var key = collections.Keys.First();
            var rows = collections[key] as List<object>;
            var models = rows?.ToList<Dictionary<string, object>, TModel>();

            return models;
        }
    }
}
