using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.Extensions;
using Newtonsoft.Json.Linq;
using OrchardCore.Forms.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts.Form
{
    public class ValidationPartDataSyncer : ContentPartDataSyncer
    {
        public override string PartName => nameof(ValidationPart);
        private static readonly Func<string, string> _formFieldsPropertyNameTransform = n => $"validation_{n}";
        public const string ForPropertyName = "For";
        public override async Task AddSyncComponents(JObject content, IDataMergeContext context)
        {
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_formFieldsPropertyNameTransform);

            JValue? forValue = (JValue?)content[ForPropertyName];
            if (forValue != null && forValue.Type != JTokenType.Null)
                context.MergeNodeCommand.Properties.Add(await context.SyncNameProvider.PropertyName(ForPropertyName), forValue.As<string>());

        }

        public override async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_formFieldsPropertyNameTransform);


            (bool matched, string failureReason) = context.DataSyncValidationHelper.StringContentPropertyMatchesNodeProperty(
                ForPropertyName,
                content,
                await context.SyncNameProvider.PropertyName(ForPropertyName),
                context.NodeWithRelationships.SourceNode!);


            return matched ? (true, "") : (false, $"{ForPropertyName} did not validate: {failureReason}");
        }
    }
}
