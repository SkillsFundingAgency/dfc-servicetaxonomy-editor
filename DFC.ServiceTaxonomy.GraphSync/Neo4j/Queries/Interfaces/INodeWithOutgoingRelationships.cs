using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Model;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces
{
    public interface INodeWithOutgoingRelationships
    {
        INode SourceNode { get; set; }
        IEnumerable<IOutgoingRelationship> OutgoingRelationships { get; set; }

        #pragma warning disable S4136
        IEnumerable<CommandRelationship> ToCommandRelationships(IGraphSyncHelper graphSyncHelper);

        IReplaceRelationshipsCommand ToReplaceRelationshipsCommand(IGraphSyncHelper graphSyncHelper);

        //todo: these belongs in a derived class in graph sync, with the current command in neo4j
        IEnumerable<CommandRelationship> ToCommandRelationships(
            IGraphSyncHelper graphSyncHelper,
            IContentItemVersion fromContentItemVersion,
            IContentItemVersion toContentItemVersion);

        IReplaceRelationshipsCommand ToReplaceRelationshipsCommand(
            IGraphSyncHelper graphSyncHelper,
            IContentItemVersion fromContentItemVersion,
            IContentItemVersion toContentItemVersion,
            bool replaceExistingRelationships = true);
        #pragma warning restore S4136
    }
}
