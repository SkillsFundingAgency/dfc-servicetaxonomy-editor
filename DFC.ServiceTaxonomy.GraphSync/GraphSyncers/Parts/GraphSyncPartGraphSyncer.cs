using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class GraphSyncPartGraphSyncer : IContentPartGraphSyncer
    {
        public string PartName => nameof(GraphSyncPart);

        public Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            object? idValue = context.GraphSyncHelper.GetIdPropertyValue(content, context.ContentItemVersion);
            if (idValue != null)
                context.MergeNodeCommand.Properties.Add(context.GraphSyncHelper.IdPropertyName(), idValue);

            return Task.CompletedTask;
        }

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            return Task.FromResult(context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                context.GraphSyncHelper.ContentIdPropertyName,
                content,
                context.GraphSyncHelper.IdPropertyName(),
                context.NodeWithOutgoingRelationships.SourceNode));
        }
    }
}
