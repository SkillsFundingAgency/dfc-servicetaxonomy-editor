using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Fields;
using DFC.ServiceTaxonomy.DataSync.Extensions;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Fields
{
    public class BooleanFieldDataSyncer : IContentFieldDataSyncer
    {
        public string FieldTypeName => "BooleanField";

        private const string ContentKey = "Value";

        public async Task AddSyncComponents(JObject contentItemField, IDataMergeContext context)
        {
            string nodePropertyName = await context.SyncNameProvider.PropertyName(context.ContentPartFieldDefinition!.Name);

            context.MergeNodeCommand.AddProperty<bool>(nodePropertyName, contentItemField, ContentKey);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject contentItemField,
            IValidateAndRepairContext context)
        {
            string nodePropertyName = await context.SyncNameProvider.PropertyName(context.ContentPartFieldDefinition!.Name);

            return context.DataSyncValidationHelper.BoolContentPropertyMatchesNodeProperty(
                ContentKey,
                contentItemField,
                nodePropertyName,
                context.NodeWithRelationships.SourceNode!);
        }
    }
}
