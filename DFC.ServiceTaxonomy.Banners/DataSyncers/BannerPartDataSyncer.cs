using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Banners.Models;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts;
using DFC.ServiceTaxonomy.DataSync.Extensions;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.Banners.DataSyncers
{
    public class BannerPartDataSyncer : ContentPartDataSyncer
    {
        public override string PartName => nameof(BannerPart);

        private static readonly Func<string, string> _bannerPropertyNameTransform = n => $"banner_{n}";


        private const string WebPageNamePropertyName = "WebPageName";
        private const string WebPageURLPropertyName = "WebPageURL";
        public override async Task AddSyncComponents(JObject content, IDataMergeContext context)
        {
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_bannerPropertyNameTransform);
            context.MergeNodeCommand.AddProperty<string>(await context.SyncNameProvider.PropertyName(WebPageNamePropertyName), content, WebPageNamePropertyName);
            context.MergeNodeCommand.AddProperty<string>(await context.SyncNameProvider.PropertyName(WebPageURLPropertyName), content, WebPageURLPropertyName);
        }

        public override async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            using var _ = context.SyncNameProvider.PushPropertyNameTransform(_bannerPropertyNameTransform);

            (bool matched, string failureReason) = context.DataSyncValidationHelper.StringContentPropertyMatchesNodeProperty(
                WebPageNamePropertyName,
                content,
                await context.SyncNameProvider.PropertyName(WebPageNamePropertyName),
                context.NodeWithRelationships.SourceNode!);

            if (!matched)
                return (false, $"{WebPageNamePropertyName} did not validate: {failureReason}");

            (matched, failureReason) = context.DataSyncValidationHelper.StringContentPropertyMatchesNodeProperty(
                WebPageURLPropertyName,
                content,
                await context.SyncNameProvider.PropertyName(WebPageURLPropertyName),
                context.NodeWithRelationships.SourceNode!);

            if (!matched)
                return (false, $"{WebPageURLPropertyName} did not validate: {failureReason}");
            return (true, "");
        }
    }
}
