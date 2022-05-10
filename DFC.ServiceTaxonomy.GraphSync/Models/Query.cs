using System.Collections.Generic;
using Microsoft.Azure.Cosmos;

namespace DFC.ServiceTaxonomy.GraphSync.Models
{
    /// <summary>
    /// An executable query, i.e. the queries' text and its parameters.
    /// </summary>
    public class Query
    {
        /// <summary>
        /// Gets the query's text.
        /// </summary>
        public string Text { get; } = string.Empty;

        /// <summary>
        /// Gets the query's parameters.
        /// </summary>
        public IDictionary<string, object> Parameters { get; } = new Dictionary<string, object>();

        public QueryDefinition? QueryDefinition { get; }

        public QueryRequestOptions? QueryRequestOptions { get; }

        public Query(string query, string parameterKey, object parameterValue, string contentType)
        {
            QueryDefinition = new QueryDefinition(query);
            QueryDefinition.WithParameter(parameterKey, parameterValue);
            QueryRequestOptions = new QueryRequestOptions { PartitionKey = new PartitionKey(contentType) };
        }

        public Query(string id, string contentType)
        {
            QueryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.id = @id0");
            QueryDefinition.WithParameter("@id0", id);

            QueryRequestOptions = new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(contentType),
                MaxItemCount = int.MaxValue
            };
        }

        /// <summary>
        /// Create a query with no query parameters.
        /// </summary>
        /// <param name="text">The query's text</param>
        public Query(string text)
        {
            Text = text;
            Parameters = new Dictionary<string, object>();
        }


        /// <summary>
        /// Create a query with no query parameters.
        /// </summary>
        /// <param name="text">The query's text</param>
        /// <param name="parameters">The query's parameters, whose values should not be changed while the query is used in a session/transaction.</param>
        public Query(string text, IDictionary<string, object> parameters)
        {
            Text = text;
            Parameters = parameters;
        }

        /// <summary>
        /// Print the query.
        /// </summary>
        /// <returns>A string representation of the query.</returns>
        public override string ToString()
        {
            return Text;
        }
    }
}
