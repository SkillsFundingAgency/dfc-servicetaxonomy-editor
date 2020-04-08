using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Cypher.Extensions;
using DFC.ServiceTaxonomy.Cypher.Models;
using DFC.ServiceTaxonomy.Cypher.Models.ResultModels;
using DFC.ServiceTaxonomy.Cypher.Queries;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Newtonsoft.Json;
using OrchardCore.Queries;

namespace DFC.ServiceTaxonomy.Cypher
{
    public class CypherQuerySource : IQuerySource
    {
        private readonly IGraphDatabase NeoGraphDatabase;

        public string Name => "Cypher";

        public CypherQuerySource(IGraphDatabase neoGraphDatabase)
        {
            NeoGraphDatabase = neoGraphDatabase ?? throw new ArgumentNullException(nameof(neoGraphDatabase));
        }

        public Query Create()
        {
            var result = new CypherQuery();

            return result;
        }

        public async Task<IQueryResults?> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
        {
            if (!(query is CypherQuery cypherQuery))
            {
                return null;
            }

            var parameterValues = BuildParameters(cypherQuery.Parameters, parameters);
            var result = new CypherQueryResult();
            //todo: need to handle nulls
            var genericCypherQuery = new GenericCypherQuery(cypherQuery.Template!, parameterValues!);
            var cypherResult = await NeoGraphDatabase.Run(genericCypherQuery);

            if (cypherResult.Any())
            {
                var collections = cypherResult.FirstOrDefault();

                if (collections != null)
                {
                    result.Items = TransformResults(cypherQuery.ResultModelType, collections);
                }
            }

            return result;
        }

        private IDictionary<string, object>? BuildParameters(string? cypherQueryParameters, IDictionary<string, object> parameters)
        {
            IDictionary<string, object>? parameterValues = null;

            if (!string.IsNullOrWhiteSpace(cypherQueryParameters))
            {
                parameterValues = JsonConvert.DeserializeObject<IDictionary<string, object>?>(cypherQueryParameters);

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
                            parameterValues[key] = value.ToObject<string[]>()!;
                        }
                    }
                }
            }

            return parameterValues;
        }

        private IEnumerable<object>? TransformResults(string? resultModelType, IDictionary<string, object> collections)
        {
            IEnumerable<object>? result = null;
            var genericTypeName = $"{typeof(IQueryResultModel).Namespace}.{resultModelType}";
            var genericType = Type.GetType(genericTypeName);

            if (genericType != null)
            {
                var methodInfo = GetType().GetMethod(nameof(TransformResultCollections), BindingFlags.Instance | BindingFlags.NonPublic)!.MakeGenericMethod(genericType);

                result = (IEnumerable<object>?)methodInfo.Invoke(this, new object[] { collections });
            }

            return result;
        }

        private List<TModel>? TransformResultCollections<TModel>(IDictionary<string, object> collections)
            where TModel : class, IQueryResultModel, new()
        {
            var key = collections.Keys.First();
            var collection = collections[key] as Dictionary<string, object>;
            var rows = collection?.Values.FirstOrDefault() as List<object>;
            var models = rows?.ToList<Dictionary<string, object>, TModel>();

            return models;
        }
    }
}
