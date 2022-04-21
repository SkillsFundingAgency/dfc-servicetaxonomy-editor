using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.Extensions;
using Newtonsoft.Json.Linq;
using OrchardCore.Alias.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts
{
    public class AliasPartDataSyncer : ContentPartDataSyncer
    {
        public override string PartName => nameof(AliasPart);

        private const string _contentAliasPropertyName = "Alias";

        //todo: configurable??
        private const string NodeAliasPropertyName = "alias_alias";

        public override Task AddSyncComponents(JObject content, IDataMergeContext context)
        {
            context.MergeNodeCommand.AddProperty<string>(NodeAliasPropertyName, content, _contentAliasPropertyName);

            return Task.CompletedTask;
        }

        public override Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            return Task.FromResult(context.DataSyncValidationHelper.StringContentPropertyMatchesNodeProperty(
                _contentAliasPropertyName,
                content,
                NodeAliasPropertyName,
                context.NodeWithRelationships.SourceNode!));
        }
    }
}
