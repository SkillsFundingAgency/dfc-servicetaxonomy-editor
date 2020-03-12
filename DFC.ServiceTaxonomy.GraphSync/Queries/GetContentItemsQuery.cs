using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Queries;
using Neo4j.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.Queries
{
    class GetContentItemsQuery : IGetContentItemsQuery
    {
        public string? QueryStatement { get; set; }

        public List<string> ValidationErrors()
        {
            var errors = new List<string>();

            if (QueryStatement == null)
                errors.Add($"{nameof(QueryStatement)} is null.");

            return errors;
        }

        public Query Query
        {
            get
            {
                this.CheckIsValid();
                return new Query(QueryStatement);
            }
        }
        public List<JObject> ProcessRecord(IRecord record)
        {
            //todo: which metadata properties should we expect the query to supply, and which to fill in?
            // fill in any that are null, e.g. Owner, Author etc.
            //probably not the place to fill those in here

            // return ((List<object>)record.Values.Values.First())
            //     .Cast<Dictionary<string, object>>();

            var values = JsonConvert.SerializeObject(record.Values.Values);
            //#pragma warning disable S1481
            List<JObject> nodes = JsonConvert.DeserializeObject<List<JObject>>(values);

            return nodes;
//             var dynamicnodes = JsonConvert.DeserializeObject<List<dynamic>>(values);
// #pragma warning restore S1481
//
//             return Enumerable.Empty<Dictionary<string, object>>();
        }
    }
}
