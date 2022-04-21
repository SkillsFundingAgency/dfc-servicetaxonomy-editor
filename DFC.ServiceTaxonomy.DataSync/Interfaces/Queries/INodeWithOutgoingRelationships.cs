using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.JsonConverters;
using DFC.ServiceTaxonomy.DataSync.Models;
using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.DataSync.Interfaces.Queries
{
    [JsonConverter(typeof(NodeWithOutgoingRelationshipsConverter))]
    public interface INodeWithOutgoingRelationships
    {
        INode SourceNode { get; set; }
        IEnumerable<IOutgoingRelationship> OutgoingRelationships { get; set; }

        #pragma warning disable S4136
        IEnumerable<CommandRelationship> ToCommandRelationships(ISyncNameProvider syncNameProvider);

        IReplaceRelationshipsCommand ToReplaceRelationshipsCommand(ISyncNameProvider syncNameProvider);

        //todo: these belongs in a derived class in graph sync, with the current command in neo4j
        IEnumerable<CommandRelationship> ToCommandRelationships(
            ISyncNameProvider syncNameProvider,
            IContentItemVersion fromContentItemVersion,
            IContentItemVersion toContentItemVersion);

        IReplaceRelationshipsCommand ToReplaceRelationshipsCommand(
            ISyncNameProvider syncNameProvider,
            IContentItemVersion fromContentItemVersion,
            IContentItemVersion toContentItemVersion,
            bool replaceExistingRelationships = true);
        #pragma warning restore S4136
    }
}
