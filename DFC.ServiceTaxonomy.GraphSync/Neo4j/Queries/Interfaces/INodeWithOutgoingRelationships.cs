using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Model;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces
{
    public interface INodeWithOutgoingRelationships
    {
        INode SourceNode { get; set; }
        IEnumerable<IOutgoingRelationship> OutgoingRelationships { get; set; }

        IEnumerable<CommandRelationship> ToCommandRelationships(IGraphSyncHelper graphSyncHelper);
    }
}
