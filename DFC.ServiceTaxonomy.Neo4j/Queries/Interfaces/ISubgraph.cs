using System.Collections.Generic;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces
{
    public interface ISubgraph
    {
        INode? SourceNode { get; }
        HashSet<INode> Nodes { get; }
        HashSet<IRelationship> Relationships  { get; }

        /// <summary>
        /// Adds the nodes and relationships from the supplied subgraph to this subgraph.
        /// </summary>
        /// <remarks>
        /// Note: currently the SourceNode is set from the supplied subgraph (this is subject to change).
        /// </remarks>
        /// <param name="subgraph">The subgraph to add.</param>
        void Add(ISubgraph subgraph);

        /// <summary>
        /// The relationships in the subgraph that are outgoing from the SourceNode.
        /// </summary>
        IEnumerable<IRelationship> OutgoingRelationships { get; }

        /// <summary>
        /// The relationships in the subgraph whose destination is the SourceNode.
        /// </summary>
        IEnumerable<IRelationship> IncomingRelationships { get; }
    }
}
