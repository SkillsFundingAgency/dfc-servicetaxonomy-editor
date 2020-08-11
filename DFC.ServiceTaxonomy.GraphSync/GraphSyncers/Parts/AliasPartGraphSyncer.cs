using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using Newtonsoft.Json.Linq;
using OrchardCore.Alias.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class AliasPartGraphSyncer : IContentPartGraphSyncer
    {
        public string PartName => nameof(AliasPart);

        private const string _contentAliasPropertyName = "Alias";

        //todo: configurable??
        private const string NodeAliasPropertyName = "alias_alias";

        public Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            context.MergeNodeCommand.AddProperty<string>(NodeAliasPropertyName, content, _contentAliasPropertyName);

            return Task.CompletedTask;
        }

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            return Task.FromResult(context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                _contentAliasPropertyName,
                content,
                NodeAliasPropertyName,
                context.NodeWithOutgoingRelationships.SourceNode));
        }
    }
}
