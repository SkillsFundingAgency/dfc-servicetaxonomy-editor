using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Cypher.Queries
{
    public class GenericCypherQuery : IQuery<IDictionary<string, object>>
    {
        private readonly string _template;
        private readonly IDictionary<string, object> _parameters;

        //todo: ideally we want to keep the ctor for injection
        public GenericCypherQuery(string template, IDictionary<string, object> parameters)
        {
            _template = template;
            _parameters = parameters;
        }

        public List<string> ValidationErrors()
        {
            return new List<string>();
        }

        public Query Query
        {
            get
            {
                return new Query(_template, _parameters);
            }
        }

        public IDictionary<string, object> ProcessRecord(IRecord record)
        {
            var kvpList = (from key in record.Keys select new KeyValuePair<string, object>(key, record[key])).ToList();

            return kvpList.ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}
