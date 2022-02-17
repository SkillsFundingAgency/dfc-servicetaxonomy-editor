using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Title.Models;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.Title.GraphSyncers
{
    public class UniqueTitlePartGraphSyncer : ContentPartGraphSyncer
    {
        public override string PartName => nameof(UniqueTitlePart);

        private static readonly Func<string, string> _uniquePropertyNameTransform = n => $"uniquetitle_{n}";


        private const string TitlePropertyName = "Title";
        public override async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_uniquePropertyNameTransform);
            context.MergeNodeCommand.AddProperty<string>(await context.SyncNameProvider.PropertyName(TitlePropertyName), content, TitlePropertyName);
        }

        public override async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_uniquePropertyNameTransform);

            (bool matched, string failureReason) = context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                TitlePropertyName,
                content,
                await context.SyncNameProvider.PropertyName(TitlePropertyName),
                context.NodeWithRelationships.SourceNode!);

            if (!matched)
                return (false, $"{TitlePropertyName} did not validate: {failureReason}");

            return (true, "");
        }
    }
}
