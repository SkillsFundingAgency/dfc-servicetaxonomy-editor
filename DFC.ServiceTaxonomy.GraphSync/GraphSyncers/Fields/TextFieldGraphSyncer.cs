using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Newtonsoft.Json.Linq;
using System.Linq;
using OrchardCore.ContentFields.Settings;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class TextFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "TextField";

        private const string ContentKey = "Text";

        public async Task AddSyncComponents(JObject contentItemField, IGraphMergeContext context)
        {
            string nodePropertyName = await context.GraphSyncHelper.PropertyName(context.ContentPartFieldDefinition!.Name);

            var settings = context.ContentPartFieldDefinition.PartDefinition.Fields.FirstOrDefault(x => x.Name == context.ContentPartFieldDefinition!.Name);

            if (settings != null && settings.GetSettings<TextFieldSettings>().Hint?.ToLower().IndexOf("##synctoarray") != -1)
            {
                var val = contentItemField[ContentKey]!.ToString().Split("\r\n");
                var array = JArray.FromObject(val);
                context.MergeNodeCommand.AddArrayProperty<string>(nodePropertyName, array, ContentKey);
                return;
            }

            context.MergeNodeCommand.AddProperty<string>(nodePropertyName, contentItemField, ContentKey);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject contentItemField,
            IValidateAndRepairContext context)
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
