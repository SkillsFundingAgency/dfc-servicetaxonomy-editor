using System.Collections.Generic;
using OrchardCore.Queries;

namespace DFC.ServiceTaxonomy.Cypher.Models
{
    public class CypherQueryResult : IQueryResults
    {
        public IEnumerable<object>? Items { get; set; }

        public CypherQueryResult() => Items = new List<object>();
    }
}
