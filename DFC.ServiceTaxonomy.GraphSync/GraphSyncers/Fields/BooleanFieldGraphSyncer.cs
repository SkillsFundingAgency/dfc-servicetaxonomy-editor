using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class BooleanFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "BooleanField";

        private const string ContentKey = "Value";

        public async Task AddSyncComponents(JsonObject contentItemField, IGraphMergeContext context)
        {
            string nodePropertyName = await context.SyncNameProvider.PropertyName(context.ContentPartFieldDefinition!.Name);

            context.MergeNodeCommand.AddProperty<bool>(nodePropertyName, contentItemField, ContentKey);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JsonObject contentItemField,
            IValidateAndRepairContext context)
        {
            string nodePropertyName = await context.SyncNameProvider.PropertyName(context.ContentPartFieldDefinition!.Name);

            return context.GraphValidationHelper.BoolContentPropertyMatchesNodeProperty(
                ContentKey,
                contentItemField,
                nodePropertyName,
                context.NodeWithRelationships.SourceNode!);
        }
    }
}
