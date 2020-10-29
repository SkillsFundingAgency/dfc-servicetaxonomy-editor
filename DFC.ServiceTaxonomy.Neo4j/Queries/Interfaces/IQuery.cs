using System.Collections.Generic;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces
{
    public interface IQuery
    {
        /// <summary>
        /// Check if the query's state is valid, i.e. it contains everything required to generate its Neo4J query.
        /// </summary>
        /// <returns>List of validation failed messages.</returns>
        List<string> ValidationErrors();

        /// <summary>
        /// Return the Neo4J query that will satisfy the query.
        /// Throws InvalidOperationException if the state is not valid.
        /// </summary>
        Query Query { get; }
    }

    public interface IQuery<out TRecord> : IQuery
    {
        /// <summary>
        /// Invoked on each result record. Returns the desired object for each IRecord.
        /// </summary>
        /// <param name="record">The record returned by Neo4J</param>
        /// <returns>The desired object for the record</returns>
        TRecord ProcessRecord(IRecord record);
    }

    // public interface IQuery<out TRecord>
    // {
    //     /// <summary>
    //     /// Check if the query's state is valid, i.e. it contains everything required to generate its Neo4J query.
    //     /// </summary>
    //     /// <returns>List of validation failed messages.</returns>
    //     List<string> ValidationErrors();
    //
    //     /// <summary>
    //     /// Return the Neo4J query that will satisfy the query.
    //     /// Throws InvalidOperationException if the state is not valid.
    //     /// </summary>
    //     Query Query { get; }
    //
    //     /// <summary>
    //     /// Invoked on each result record. Returns the desired object for each IRecord.
    //     /// </summary>
    //     /// <param name="record">The record returned by Neo4J</param>
    //     /// <returns>The desired object for the record</returns>
    //     TRecord ProcessRecord(IRecord record);
    // }
}
