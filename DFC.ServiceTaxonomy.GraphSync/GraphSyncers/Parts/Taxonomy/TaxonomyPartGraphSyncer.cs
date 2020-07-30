﻿using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using Newtonsoft.Json.Linq;
using OrchardCore.Taxonomies.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Taxonomy
{
    public class TaxonomyPartGraphSyncer : ITaxonomyPartGraphSyncer
    {
        private readonly ITaxonomyPartEmbeddedContentItemsGraphSyncer _taxonomyPartEmbeddedContentItemsGraphSyncer;
        public string PartName => nameof(TaxonomyPart);

        private const string ContainerName = "Terms";
        internal const string TermContentTypePropertyName = "TermContentType";

        public TaxonomyPartGraphSyncer(
            ITaxonomyPartEmbeddedContentItemsGraphSyncer taxonomyPartEmbeddedContentItemsGraphSyncer)
        {
            _taxonomyPartEmbeddedContentItemsGraphSyncer = taxonomyPartEmbeddedContentItemsGraphSyncer;
        }

        public async Task AllowSync(JObject content, IGraphMergeContext context, IAllowSyncResult allowSyncResult)
        {
            await _taxonomyPartEmbeddedContentItemsGraphSyncer.AllowSync((JArray?)content[ContainerName], context, allowSyncResult);
        }

        public async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            // useful if there are no terms yet?
            context.MergeNodeCommand.AddProperty<string>(TermContentTypePropertyName, content);

            await _taxonomyPartEmbeddedContentItemsGraphSyncer.AddSyncComponents((JArray?)content[ContainerName], context);
        }

        //todo: we now need to validate any 2 way incoming relationships we created
        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            (bool validated, string failureReason) =
                await _taxonomyPartEmbeddedContentItemsGraphSyncer.ValidateSyncComponent(
                    (JArray?)content[ContainerName], context);

            if (!validated)
                return (validated, failureReason);

            return context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                TermContentTypePropertyName,
                content,
                TermContentTypePropertyName,
                context.NodeWithOutgoingRelationships.SourceNode);
        }
    }
}
