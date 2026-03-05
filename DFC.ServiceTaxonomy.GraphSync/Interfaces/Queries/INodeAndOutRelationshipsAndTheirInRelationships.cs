using System.Collections.Generic;
using System.Text.Json.Serialization;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.JsonConverters;
using DFC.ServiceTaxonomy.GraphSync.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Interfaces.Queries
{
    [JsonConverter(typeof(NodeAndOutRelationshipsAndTheirInRelationshipsConverter))]
    public interface INodeAndOutRelationshipsAndTheirInRelationships
    {
        INode SourceNode { get; set; }
        public IEnumerable<(IOutgoingRelationship outgoingRelationship, IEnumerable<IOutgoingRelationship> incomingRelationships)> OutgoingRelationships { get; set; }

        IEnumerable<CommandRelationship> ToCommandRelationships(ISyncNameProvider syncNameProvider);
    }
}
