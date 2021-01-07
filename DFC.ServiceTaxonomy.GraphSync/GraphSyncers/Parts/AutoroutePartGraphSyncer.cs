using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Autoroute.Models;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class AutoroutePartGraphSyncer : ContentPartGraphSyncer
    {
        public override string PartName => nameof(AutoroutePart);

        private const string _contentTitlePropertyName = "Path";
        private const string NodeTitlePropertyName = "autoroute_path";

        public override Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            context.MergeNodeCommand.AddProperty<string>(NodeTitlePropertyName, content, _contentTitlePropertyName);

            return Task.CompletedTask;
        }

        public override Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            return Task.FromResult(context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                _contentTitlePropertyName,
                content,
                NodeTitlePropertyName,
                context.NodeWithRelationships.SourceNode!));
        }
    }
}
