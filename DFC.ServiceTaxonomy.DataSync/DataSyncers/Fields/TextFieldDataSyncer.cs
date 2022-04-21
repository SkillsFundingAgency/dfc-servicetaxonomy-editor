using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Fields;
using DFC.ServiceTaxonomy.DataSync.Extensions;
using DFC.ServiceTaxonomy.DataSync.OrchardCore.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Settings;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Fields
{
    public class TextFieldDataSyncer : IContentFieldDataSyncer
    {
        public string FieldTypeName => "TextField";

        private const string ContentKey = "Text";

        private const string SyncToArrayFlag = "##synctoarray";

        public async Task AddSyncComponents(JObject contentItemField, IDataMergeContext context)
        {
            string nodePropertyName = await context.SyncNameProvider.PropertyName(context.ContentPartFieldDefinition!.Name);

            if (SyncMultilineToArray(context.ContentPartFieldDefinition))
            {
                context.MergeNodeCommand.AddArrayPropertyFromMultilineString(
                    nodePropertyName, contentItemField, ContentKey);
                return;
            }

            context.MergeNodeCommand.AddProperty<string>(nodePropertyName, contentItemField, ContentKey);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject contentItemField,
            IValidateAndRepairContext context)
        {
            string nodePropertyName = await context.SyncNameProvider.PropertyName(context.ContentPartFieldDefinition!.Name);

            if (SyncMultilineToArray(context.ContentPartFieldDefinition))
            {
                return context.DataSyncValidationHelper.ContentMultilineStringPropertyMatchesNodeProperty(
                    ContentKey,
                    contentItemField,
                    nodePropertyName,
                    context.NodeWithRelationships.SourceNode!);
            }

            return context.DataSyncValidationHelper.StringContentPropertyMatchesNodeProperty(
                ContentKey,
                contentItemField,
                nodePropertyName,
                context.NodeWithRelationships.SourceNode!);
        }

        private bool SyncMultilineToArray(IContentPartFieldDefinition contentPartFieldDefinition)
        {
            string? hint = contentPartFieldDefinition.GetSettings<TextFieldSettings>().Hint;
            return hint != null && hint.ToLower().IndexOf(SyncToArrayFlag, StringComparison.Ordinal) != -1;
        }
    }
}
