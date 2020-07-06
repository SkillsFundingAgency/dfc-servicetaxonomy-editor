using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.ValidateAndRepair;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class TextFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "TextField";

        private const string ContentKey = "Text";

        public async Task AddSyncComponents(JObject contentItemField, IGraphMergeContext context)
        {
            string nodePropertyName = await context.GraphSyncHelper.PropertyName(context.ContentPartFieldDefinition!.Name);

            context.MergeNodeCommand.AddProperty<string>(nodePropertyName, contentItemField, ContentKey);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject contentItemField,
            ValidateAndRepairContext context)
        {
            string nodePropertyName = await context.GraphSyncHelper.PropertyName(context.ContentPartFieldDefinition!.Name);

            return context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                ContentKey,
                contentItemField,
                nodePropertyName,
                context.NodeWithOutgoingRelationships.SourceNode);
        }
    }
}
