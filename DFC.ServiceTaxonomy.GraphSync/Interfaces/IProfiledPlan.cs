using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    //
    // Summary:
    //     This is the same as a regular Neo4j.Driver.IPlan - except this plan has been
    //     executed, meaning it also contains detailed information about how much work each
    //     step of the plan incurred on the database.
    public interface IProfiledPlan : IPlan
    {
        //
        // Summary:
        //     Gets the number of times this part of the plan touched the underlying data stores
        long DbHits { get; }
        //
        // Summary:
        //     Gets the number of records this part of the plan produced
        long Records { get; }
        //
        // Summary:
        //     Gets number of page cache hits caused by executing the associated execution step.
        long PageCacheHits { get; }
        //
        // Summary:
        //     Gets number of page cache misses caused by executing the associated execution
        //     step
        long PageCacheMisses { get; }
        //
        // Summary:
        //     Gets the ratio of page cache hits to total number of lookups or 0 if no data
        //     is available
        double PageCacheHitRatio { get; }
        //
        // Summary:
        //     Gets amount of time spent in the associated execution step.
        long Time { get; }
        //
        // Summary:
        //     Gets if the number page cache hits and misses and the ratio was recorded.
        bool HasPageCacheStats { get; }

        //
        // Summary:
        //     Gets zero or more child profiled plans. A profiled plan is a tree, where each
        //     child is another profiled plan. The children are where this part of the plan
        //     gets its input records - unless this is an Neo4j.Driver.IPlan.OperatorType that
        //     introduces new records on its own.
        new IList<IProfiledPlan> Children { get; }
    }
}
