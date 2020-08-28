using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class HtmlFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "HtmlField";

        private const string ContentKey = "Html";

        public async Task AddSyncComponents(JObject contentItemField, IGraphMergeContext context)
        {
            JValue? value = (JValue?)contentItemField[ContentKey];
            if (value == null || value.Type == JTokenType.Null)
                return;

            context.MergeNodeCommand.Properties.Add(
                await context.SyncNameProvider.PropertyName(context.ContentPartFieldDefinition!.Name),
                value.As<string>());
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject contentItemField,
            IValidateAndRepairContext context)
        {
            string nodePropertyName = await context.SyncNameProvider.PropertyName(context.ContentPartFieldDefinition!.Name);

            return context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                ContentKey,
                contentItemField,
                nodePropertyName,
                context.NodeWithOutgoingRelationships.SourceNode);
        }

        public Task AddRelationship(IDescribeRelationshipsContext itemContext)
        {
            return Task.CompletedTask;
        }
    }
}
