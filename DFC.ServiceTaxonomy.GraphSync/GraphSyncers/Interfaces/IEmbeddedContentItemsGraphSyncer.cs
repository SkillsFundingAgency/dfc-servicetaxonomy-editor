using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IEmbeddedContentItemsGraphSyncer
    {
        Task AddSyncComponents(dynamic content, IReplaceRelationshipsCommand replaceRelationshipsCommand);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts,
            string endpoint);
    }
}
