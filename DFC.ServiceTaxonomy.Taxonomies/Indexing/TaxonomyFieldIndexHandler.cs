using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Taxonomies.Fields;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Indexing;

namespace DFC.ServiceTaxonomy.Taxonomies.Indexing
{
    public class TaxonomyFieldIndexHandler : ContentFieldIndexHandler<TaxonomyField>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TaxonomyFieldIndexHandler> _logger;

        public TaxonomyFieldIndexHandler(IServiceProvider serviceProvider, ILogger<TaxonomyFieldIndexHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public override async Task BuildIndexAsync(TaxonomyField field, BuildFieldIndexContext context)
        {
            // TODO: Also add the parents of each term, probably as a separate field
            _logger.LogInformation("BuildIndexAsync : field {field} ", field);
            var options = context.Settings.ToOptions();
            options |= DocumentIndexOptions.Store;

            // Directly selected term ids are added to the default field name
            foreach (var contentItemId in field.TermContentItemIds)
            {
                foreach (var key in context.Keys)
                {
                    _logger.LogInformation("BuildIndexAsync set : key {key} contentItemId {contentItemId} ", key, contentItemId);
                    context.DocumentIndex.Set(key, contentItemId, options);
                }
            }

            // Inherited term ids are added to a distinct field, prefixed with "Inherited"
            var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
            var taxonomy = await contentManager.GetAsync(field.TaxonomyContentItemId);

            var inheritedContentItems = new List<ContentItem>();
            foreach (var contentItemId in field.TermContentItemIds)
            {

                TaxonomyOrchardHelperExtensions.FindTermHierarchy(taxonomy.Content.TaxonomyPart.Terms as JArray, contentItemId, inheritedContentItems);
            }

            foreach (var key in context.Keys)
            {
                foreach (var contentItem in inheritedContentItems)
                {
                    context.DocumentIndex.Set(key + ".Inherited", contentItem.ContentItemId, options);
                }
            }
        }
    }
}
