namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    //
    // Summary:
    //     Represents the changes to the database made as a result of a query being run.
    public interface ICounters
    {
        //
        // Summary:
        //     Gets whether there were any updates at all, eg. any of the counters are greater
        //     than 0.
        //
        // Value:
        //     Returns true if the query made any updates, false otherwise.
        bool ContainsUpdates { get; }
        //
        // Summary:
        //     Gets the number of nodes created.
        int NodesCreated { get; }
        //
        // Summary:
        //     Gets the number of nodes deleted.
        int NodesDeleted { get; }
        //
        // Summary:
        //     Gets the number of relationships created.
        int RelationshipsCreated { get; }
        //
        // Summary:
        //     Gets the number of relationships deleted.
        int RelationshipsDeleted { get; }
        //
        // Summary:
        //     Gets the number of properties (on both nodes and relationships) set.
        int PropertiesSet { get; }
        //
        // Summary:
        //     Gets the number of labels added to nodes.
        int LabelsAdded { get; }
        //
        // Summary:
        //     Gets the number of labels removed from nodes.
        int LabelsRemoved { get; }
        //
        // Summary:
        //     Gets the number of indexes added to the schema.
        int IndexesAdded { get; }
        //
        // Summary:
        //     Gets the number of indexes removed from the schema.
        int IndexesRemoved { get; }
        //
        // Summary:
        //     Gets the number of constraints added to the schema.
        int ConstraintsAdded { get; }
        //
        // Summary:
        //     Gets the number of constraints removed from the schema.
        int ConstraintsRemoved { get; }
        //
        // Summary:
        //     Gets the number of system updates performed by this query.
        int SystemUpdates { get; }
        //
        // Summary:
        //     If the query updated the system graph in any way, this method will return true.
        bool ContainsSystemUpdates { get; }
    }
}
