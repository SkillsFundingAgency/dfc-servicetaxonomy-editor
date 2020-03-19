using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IMergeGraphSyncer
    {
        Task<IMergeNodeCommand?> SyncToGraph(string contentType, string contentItemId, JObject content, DateTime? createdUtc, DateTime? modifiedUtc);
    }
}
