using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Model;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Models
{
    public interface INodeAndOutRelationshipsAndTheirInRelationships
    {
        INode SourceNode { get; set; }
        public IEnumerable<(IOutgoingRelationship outgoingRelationship, IEnumerable<IOutgoingRelationship> incomingRelationships)> OutgoingRelationships { get; set; }

        IEnumerable<CommandRelationship> ToCommandRelationships(IGraphSyncHelper graphSyncHelper);
    }
}
