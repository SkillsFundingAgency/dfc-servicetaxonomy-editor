using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Taxonomies.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class TermPartGraphSyncer : ITermPartGraphSyncer
    {
        public string PartName => nameof(TermPart);

        private const string TaxonomyContentItemId = "TaxonomyContentItemId";

        private readonly IServiceProvider _serviceProvider;

        public TermPartGraphSyncer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            string? taxonomyContentItemId = (string?)content[TaxonomyContentItemId];
            if (taxonomyContentItemId == null)
                throw new GraphSyncException($"{PartName} is missing {TaxonomyContentItemId}.");

            ContentItem contentItem = await context.ContentItemVersion.GetContentItem(context.ContentManager, taxonomyContentItemId);

            IGraphSyncHelper termGraphSyncHelper = _serviceProvider.GetRequiredService<IGraphSyncHelper>();
            termGraphSyncHelper.ContentType = contentItem.ContentType;

            //todo: override/extension that takes a contentitem
            context.ReplaceRelationshipsCommand.AddRelationshipsTo(
                //todo: go through graphSyncHelper
                $"has{contentItem.ContentType}",
                null,
                await termGraphSyncHelper.NodeLabels(),
                termGraphSyncHelper.IdPropertyName(),
                termGraphSyncHelper.GetIdPropertyValue(contentItem.Content.GraphSyncPart, context.ContentItemVersion));
        }

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            throw new NotImplementedException();
        }
    }
}
