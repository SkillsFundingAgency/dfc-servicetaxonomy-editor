using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class BooleanFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "BooleanField";

        private const string ContentKey = "Value";

        public async Task AddSyncComponents(JObject contentItemField, IGraphMergeContext context)
        {
            string nodePropertyName = await context.GraphSyncHelper.PropertyName(context.ContentPartFieldDefinition!.Name);

            context.MergeNodeCommand.AddProperty<bool>(nodePropertyName, contentItemField, ContentKey);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject contentItemField,
            IValidateAndRepairContext context)
        {
            string nodePropertyName = await context.GraphSyncHelper.PropertyName(context.ContentPartFieldDefinition!.Name);

            return context.GraphValidationHelper.BoolContentPropertyMatchesNodeProperty(
                ContentKey,
                contentItemField,
                nodePropertyName,
                context.NodeWithOutgoingRelationships.SourceNode);
        }

        public Task AddRelationship(IDescribeRelationshipsContext parentContext)
        {
            return Task.CompletedTask;
        }
    }
}
