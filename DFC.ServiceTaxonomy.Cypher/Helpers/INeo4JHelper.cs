using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Cypher.Helpers
{
    public interface INeo4JHelper
    {
        Task<object> ExecuteCypherQueryInNeo4JAsync(string query, IDictionary<string, object> statementParameters);

        Task<IResultSummary> GetResultSummaryAsync();
    }
}
