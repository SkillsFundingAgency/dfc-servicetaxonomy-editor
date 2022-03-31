using System;

namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    public interface IRelationship : IEntity, IEquatable<IRelationship>
    {
        //
        // Summary:
        //     Gets the type of the relationship.
        string Type { get; set; }

        //
        // Summary:
        //     Gets the id of the start node of the relationship.
        long StartNodeId { get; }

        //
        // Summary:
        //     Gets the id of the end node of the relationship.
        long EndNodeId { get; }
    }
}
