﻿using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using DFC.ServiceTaxonomy.Taxonomies.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class TermPartGraphSyncer : ContentPartGraphSyncer, ITermPartGraphSyncer
    {
        public override string PartName => nameof(TermPart);

        private const string TaxonomyContentItemId = "TaxonomyContentItemId";

        private readonly IServiceProvider _serviceProvider;

        public TermPartGraphSyncer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            string? taxonomyContentItemId = (string?)content[TaxonomyContentItemId];
            if (taxonomyContentItemId == null)
                throw new GraphSyncException($"{PartName} is missing {TaxonomyContentItemId}.");

            ContentItem contentItem = await context.ContentItemVersion.GetContentItem(context.ContentManager, taxonomyContentItemId);

            ISyncNameProvider termSyncNameProvider = _serviceProvider.GetRequiredService<ISyncNameProvider>();
            termSyncNameProvider.ContentType = contentItem.ContentType;

            //todo: override/extension that takes a contentitem
            context.ReplaceRelationshipsCommand.AddRelationshipsTo(
                //todo: go through syncNameProvider
                $"has{contentItem.ContentType}",
                null,
                await termSyncNameProvider.NodeLabels(),
                termSyncNameProvider.IdPropertyName(),
                termSyncNameProvider.GetIdPropertyValue(contentItem.Content.GraphSyncPart, context.ContentItemVersion));
        }

        public override Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            throw new NotImplementedException();
        }
    }
}
