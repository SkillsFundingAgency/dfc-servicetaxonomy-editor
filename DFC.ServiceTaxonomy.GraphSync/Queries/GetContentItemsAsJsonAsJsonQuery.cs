using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Queries;
using Neo4j.Driver;
using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.GraphSync.Queries
{
    //todo: rename, doesn't get contentitems!
    class GetContentItemsAsJsonAsJsonQuery : IGetContentItemsAsJsonQuery
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
        public string ProcessRecord(IRecord record)
        {
            return JsonConvert.SerializeObject(record.Values.Values);
            // string values = JsonConvert.SerializeObject(record.Values.Values);
            // return JsonConvert.DeserializeObject<List<JObject>>(values);
        }
    }
}
