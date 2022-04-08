﻿using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using Newtonsoft.Json.Linq;
using OrchardCore.Forms.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Form
{
    public class ValidationPartGraphSyncer : ContentPartGraphSyncer
    {
        public override string PartName => nameof(ValidationPart);
        private static readonly Func<string, string> _formFieldsPropertyNameTransform = n => $"validation_{n}";
        public const string ForPropertyName = "For";
        public override async Task AddSyncComponents(JObject content, IGraphMergeContext context)
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


            (bool matched, string failureReason) = context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                ForPropertyName,
                content,
                await context.SyncNameProvider.PropertyName(ForPropertyName),
                context.NodeWithRelationships.SourceNode!);


            return matched ? (true, "") : (false, $"{ForPropertyName} did not validate: {failureReason}");
        }
    }
}
