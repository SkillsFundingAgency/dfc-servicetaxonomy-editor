using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Settings;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class TextFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "TextField";

        private const string ContentKey = "Text";

        private const string SyncToArrayFlag = "##synctoarray";

        public async Task AddSyncComponents(JObject contentItemField, IGraphMergeContext context)
        {
            string nodePropertyName = await context.GraphSyncHelper.PropertyName(context.ContentPartFieldDefinition!.Name);

            string? hint = context.ContentPartFieldDefinition.GetSettings<TextFieldSettings>().Hint;
            if (hint != null && hint.ToLower().IndexOf(SyncToArrayFlag, StringComparison.Ordinal) != -1)
            {
                context.MergeNodeCommand.AddArrayPropertyFromMultilineString(
                    nodePropertyName, contentItemField, ContentKey);
                return;
            }

            context.MergeNodeCommand.AddProperty<string>(nodePropertyName, contentItemField, ContentKey);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject contentItemField,
            IValidateAndRepairContext context)
        {
            string nodePropertyName = await context.GraphSyncHelper.PropertyName(context.ContentPartFieldDefinition!.Name);

            //todo: fix validating split string
            return context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                ContentKey,
                contentItemField,
                nodePropertyName,
                context.NodeWithOutgoingRelationships.SourceNode);
        }
    }
}
