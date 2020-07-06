using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.Autoroute.Models;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.ValidateAndRepair;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class AutoroutePartGraphSyncer : IContentPartGraphSyncer
    {
        public string PartName => nameof(AutoroutePart);

        private const string _contentTitlePropertyName = "Path";

        //todo: configurable??
        public const string NodeTitlePropertyName = "autoroute_path";

        public Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            context.MergeNodeCommand.AddProperty<string>(NodeTitlePropertyName, content, _contentTitlePropertyName);

            return Task.CompletedTask;
        }

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            ValidateAndRepairContext context)
        {
            return Task.FromResult(context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                _contentTitlePropertyName,
                content,
                NodeTitlePropertyName,
                context.NodeWithOutgoingRelationships.SourceNode));
        }
    }
}
