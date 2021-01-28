using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using Newtonsoft.Json.Linq;
using OrchardCore.Alias.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class AliasPartGraphSyncer : ContentPartGraphSyncer
    {
        public override string PartName => nameof(AliasPart);

        private const string _contentAliasPropertyName = "Alias";

        //todo: configurable??
        private const string NodeAliasPropertyName = "alias_alias";

        public override Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            context.MergeNodeCommand.AddProperty<string>(NodeAliasPropertyName, content, _contentAliasPropertyName);

            return Task.CompletedTask;
        }

        public override Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            return Task.FromResult(context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                _contentAliasPropertyName,
                content,
                NodeAliasPropertyName,
                context.NodeWithRelationships.SourceNode!));
        }
    }
}
