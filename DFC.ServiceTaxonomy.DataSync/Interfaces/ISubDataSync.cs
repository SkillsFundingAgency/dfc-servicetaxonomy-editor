using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.DataSync.Interfaces
{
    public interface ISubDataSync
    {
        INode? SourceNode { get; }
        HashSet<INode> Nodes { get; }
        HashSet<IRelationship> Relationships { get; }

        /// <summary>
        /// Adds the nodes and relationships from the supplied subDataSync to this subDataSync.
        /// </summary>
        /// <remarks>
        /// Note: currently the SourceNode is set from the supplied subDataSync (this is subject to change).
        /// </remarks>
        /// <param name="subDataSync">The subDataSync to add.</param>
        void Add(ISubDataSync subDataSync);

        /// <summary>
        /// The relationships in the subDataSync that are outgoing from the SourceNode.
        /// </summary>
        IEnumerable<IRelationship> OutgoingRelationships { get; }

        /// <summary>
        /// The relationships in the subDataSync whose destination is the SourceNode.
        /// </summary>
        IEnumerable<IRelationship> IncomingRelationships { get; }
    }
}
