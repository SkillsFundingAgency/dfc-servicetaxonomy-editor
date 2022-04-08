using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.Models
{
    /// <summary>
    /// An executable query, i.e. the queries' text and its parameters.
    /// </summary>
    public class Query
    {
        /// <summary>
        /// Create a query with no query parameters.
        /// </summary>
        /// <param name="text">The query's text</param>
        public Query(string text)
        {
            Text = text;
            Parameters = new Dictionary<string, object>();
        }

        //// <summary>
        /// Create a query with no query parameters.
        /// </summary>
        /// <param name="text">The query's text</param>
        /// <param name="parameters">The query parameters, specified as an object which is then converted into key-value pairs.</param>
        public Query(string text, object parameters)
        {
            Text = text;
            Parameters = (Dictionary<string, object>)parameters;
        }

        //// <summary>
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
        /// Gets the query's text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the query's parameters.
        /// </summary>
        public IDictionary<string, object> Parameters { get; }

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
