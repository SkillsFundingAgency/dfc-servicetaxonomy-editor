using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.Extensions;
using Newtonsoft.Json.Linq;
using OrchardCore.Autoroute.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts
{
    public class AutoroutePartDataSyncer : ContentPartDataSyncer
    {
        public override string PartName => nameof(AutoroutePart);

        private const string _contentTitlePropertyName = "Path";
        private const string NodeTitlePropertyName = "autoroute_path";

        public override Task AddSyncComponents(JObject content, IDataMergeContext context)
        {
            context.MergeNodeCommand.AddProperty<string>(NodeTitlePropertyName, content, _contentTitlePropertyName);

            return Task.CompletedTask;
        }

        public override Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            return Task.FromResult(context.DataSyncValidationHelper.StringContentPropertyMatchesNodeProperty(
                _contentTitlePropertyName,
                content,
                NodeTitlePropertyName,
                context.NodeWithRelationships.SourceNode!));
        }
    }
}
