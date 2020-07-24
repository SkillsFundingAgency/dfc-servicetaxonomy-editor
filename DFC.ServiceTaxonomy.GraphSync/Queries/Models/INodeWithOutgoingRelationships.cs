using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Queries.Models
{
    public interface INodeWithOutgoingRelationships
    {
        INode SourceNode { get; set; }
        IEnumerable<IOutgoingRelationship> OutgoingRelationships { get; set; }

        IEnumerable<CommandRelationship> ToCommandRelationships(IGraphSyncHelper graphSyncHelper);
    }
}
