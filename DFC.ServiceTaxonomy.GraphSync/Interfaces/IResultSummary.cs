﻿using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Enums;
using DFC.ServiceTaxonomy.GraphSync.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    public interface IResultSummary
    {
        //
        // Summary:
        //     Gets query that has been executed.
        Query Query { get; }
        //
        // Summary:
        //     Gets statistics counts for the query.
        ICounters Counters { get; }
        //
        // Summary:
        //     Gets type of query that has been executed.
        QueryType QueryType { get; }
        //
        // Summary:
        //     Gets if the result contained a query plan or not, i.e. is the summary of a Cypher
        //     PROFILE or EXPLAIN query.
        bool HasPlan { get; }
        //
        // Summary:
        //     Gets if the result contained profiling information or not, i.e. is the summary
        //     of a Cypher PROFILE query.
        bool HasProfile { get; }
        //
        // Summary:
        //     Gets query plan for the executed query if available, otherwise null.
        //
        // Remarks:
        //     This describes how the database will execute your query.
        IPlan Plan { get; }
        //
        // Summary:
        //     Gets profiled query plan for the executed query if available, otherwise null.
        //
        // Remarks:
        //     This describes how the database did execute your query. If the query you executed
        //     (Neo4j.Driver.IResultSummary.HasProfile was profiled), the query plan will contain
        //     detailed information about what each step of the plan did. That more in-depth
        //     version of the query plan becomes available here.
        IProfiledPlan Profile { get; }
        //
        // Summary:
        //     Gets a list of notifications produced while executing the query. The list will
        //     be empty if no notifications produced while executing the query.
        //
        // Remarks:
        //     A list of notifications that might arise when executing the query. Notifications
        //     can be warnings about problematic queries or other valuable information that
        //     can be presented in a client. Unlike failures or errors, notifications do not
        //     affect the execution of a query.
        IList<INotification> Notifications { get; }
        //
        // Summary:
        //     The time it took the server to make the result available for consumption. Default
        //     to -00:00:00.001 if the server version does not support this field in summary.
        //
        // Remarks:
        //     Field introduced in Neo4j 3.1.
        TimeSpan ResultAvailableAfter { get; }
        //
        // Summary:
        //     The time it took the server to consume the result. Default to -00:00:00.001 if
        //     the server version does not support this field in summary.
        //
        // Remarks:
        //     Field introduced in Neo4j 3.1.
        TimeSpan ResultConsumedAfter { get; }

        //
        // Summary:
        //     Get some basic information of the server where the query is carried out
        IServerInfo Server { get; }

        //
        // Summary:
        //     Get the database information that this summary is generated from.
        IDatabaseInfo Database { get; }
    }
}
