using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Exceptions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.DataSync.Extensions;
using DFC.ServiceTaxonomy.Taxonomies.Models;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts
{
    public class TermPartDataSyncer : ContentPartDataSyncer, ITermPartDataSyncer
    {
        public override string PartName => nameof(TermPart);

        private const string TaxonomyContentItemId = "TaxonomyContentItemId";

        private readonly IServiceProvider _serviceProvider;

        public TermPartDataSyncer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override async Task AddSyncComponents(JObject content, IDataMergeContext context)
        {
            string? taxonomyContentItemId = (string?)content[TaxonomyContentItemId];
            if (taxonomyContentItemId == null)
                throw new DataSyncException($"{PartName} is missing {TaxonomyContentItemId}.");

            //todo: check for null
            ContentItem? contentItem = await context.ContentItemVersion.GetContentItem(context.ContentManager, taxonomyContentItemId);

            ISyncNameProvider termSyncNameProvider = _serviceProvider.GetSyncNameProvider(contentItem!.ContentType);

            //todo: override/extension that takes a contentitem
            context.ReplaceRelationshipsCommand.AddRelationshipsTo(
                //todo: go through syncNameProvider
                $"has{contentItem.ContentType}",
                null,
                await termSyncNameProvider.NodeLabels(),
                termSyncNameProvider.IdPropertyName(),
                termSyncNameProvider.GetNodeIdPropertyValue(contentItem.Content.GraphSyncPart, context.ContentItemVersion));
        }

        //todo: would need to add AddSyncComponentsDetaching if we start using this

        public override Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            throw new NotImplementedException();
        }
    }
}
