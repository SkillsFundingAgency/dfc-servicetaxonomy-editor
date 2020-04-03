using System.Collections.Generic;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces
{
    public interface ICommand
    {
        /// <summary>
        /// Check if the command's state is valid, i.e. it contains everything required to generate its query.
        /// </summary>
        /// <returns>List of validation failed messages.</returns>
        List<string> ValidationErrors();

        /// <summary>
        /// Return the Neo4J query that will satisfy the command.
        /// Throws InvalidOperationException if the state is not valid.
        /// </summary>
        Query Query { get; }

        /// <summary>
        /// Check if the result is valid, and throw a CommandValidationException if not.
        /// Examples of what you might want to check:
        ///   result from query
        ///   counters
        ///   notifications
        /// </summary>
        /// <param name="records"></param>
        /// <param name="resultSummary"></param>
        void ValidateResults(List<IRecord> records, IResultSummary resultSummary);
    }
}
