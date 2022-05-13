using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    //
    // Summary:
    //     This describes the Plan that the database planner produced and used (or will
    //     use) to execute your query. This can be extremely helpful in understanding what
    //     a query is doing, and how to optimize it. For more details, see the Neo4j Manual.
    //     The plan for the query is a tree of plans - each sub-tree containing zero or
    //     more child plans. The query starts with the root plan. Each sub-plan is of a
    //     specific Neo4j.Driver.IPlan.OperatorType, which describes what that part of the
    //     plan does - for instance, perform an index lookup or filter results. The Neo4j
    //     Manual contains a reference of the available operator types, and these may differ
    //     across Neo4j versions.
    public interface IPlan
    {
        //
        // Summary:
        //     Gets the operation this plan is performing.
        string OperatorType { get; }
        //
        // Summary:
        //     Gets the arguments for the Neo4j.Driver.IPlan.OperatorType used. Many Neo4j.Driver.IPlan.OperatorType
        //     have arguments defining their specific behavior. This map contains those arguments.
        IDictionary<string, object> Arguments { get; }
        //
        // Summary:
        //     Gets a list of identifiers used by this plan. Identifiers used by this part of
        //     the plan. These can be both identifiers introduce by you, or automatically generated
        //     identifiers.
        IList<string> Identifiers { get; }
        //
        // Summary:
        //     Gets zero or more child plans. A plan is a tree, where each child is another
        //     plan. The children are where this part of the plan gets its input records - unless
        //     this is an Neo4j.Driver.IPlan.OperatorType that introduces new records on its
        //     own.
        IList<IPlan> Children { get; }
    }
}
