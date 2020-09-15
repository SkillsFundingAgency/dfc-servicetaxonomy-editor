using System.Collections.Generic;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces
{
    public interface INodeWithIncomingRelationships
    {
        INode SourceNode { get; set; }
        IEnumerable<IOutgoingRelationship> IncomingRelationships { get; set; }
    }
}
