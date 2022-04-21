using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.JsonConverters;
using DFC.ServiceTaxonomy.DataSync.Models;
using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.DataSync.Interfaces.Queries
{
    [JsonConverter(typeof(NodeAndOutRelationshipsAndTheirInRelationshipsConverter))]
    public interface INodeAndOutRelationshipsAndTheirInRelationships
    {
        INode SourceNode { get; set; }
        public IEnumerable<(IOutgoingRelationship outgoingRelationship, IEnumerable<IOutgoingRelationship> incomingRelationships)> OutgoingRelationships { get; set; }

        IEnumerable<CommandRelationship> ToCommandRelationships(ISyncNameProvider syncNameProvider);
    }
}
