using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Taxonomies.Fields;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.Taxonomies.Indexing
{
    public class TaxonomyIndex : MapIndex
    {
        public string TaxonomyContentItemId { get; set; }
        public string ContentItemId { get; set; }
        public string ContentType { get; set; }
        public string ContentPart { get; set; }
        public string ContentField { get; set; }
        public string TermContentItemId { get; set; }
    }

    public class TaxonomyIndexProvider : IndexProvider<ContentItem>, IScopedIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HashSet<string> _ignoredTypes = new HashSet<string>();
        private IContentDefinitionManager _contentDefinitionManager;

        public TaxonomyIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<TaxonomyIndex>()
                .Map(contentItem =>
                {
                    // allow draft content items to be indexed
                    //todo: do we still need to allow them, now we use published only?
                    // if (!contentItem.IsPublished())
                    // {
                    //     return null;
                    // }

                    // Can we safely ignore this content item?
                    if (_ignoredTypes.Contains(contentItem.ContentType))
                    {
                        return null;
                    }

                    // Lazy initialization because of ISession cyclic dependency
                    _contentDefinitionManager = _contentDefinitionManager ?? _serviceProvider.GetRequiredService<IContentDefinitionManager>();

                    // Search for Taxonomy fields
                    var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                    // This can occur when content items become orphaned, particularly layer widgets when a layer is removed, before its widgets have been unpublished.
                    if (contentTypeDefinition == null)
                    {
                        _ignoredTypes.Add(contentItem.ContentType);
                        return null;
                    }

                    var fieldDefinitions = contentTypeDefinition
                        .Parts.SelectMany(x => x.PartDefinition.Fields.Where(f => f.FieldDefinition.Name == nameof(TaxonomyField)))
                        .ToArray();

                    // This type doesn't have any TaxonomyField, ignore it
                    if (fieldDefinitions.Length == 0)
                    {
                        _ignoredTypes.Add(contentItem.ContentType);
                        return null;
                    }

                    var results = new List<TaxonomyIndex>();

                    // Get all field values
                    foreach (var fieldDefinition in fieldDefinitions)
                    {
                        var jPart = (JObject)contentItem.Content[fieldDefinition.PartDefinition.Name];

                        if (jPart == null)
                        {
                            continue;
                        }

                        var jField = (JObject)jPart[fieldDefinition.Name];

                        if (jField == null)
                        {
                            continue;
                        }

                        var field = jField.ToObject<TaxonomyField>();

                        foreach (var termContentItemId in field.TermContentItemIds)
                        {
                            results.Add(new TaxonomyIndex
                            {
                                TaxonomyContentItemId = field.TaxonomyContentItemId,
                                ContentItemId = contentItem.ContentItemId,
                                ContentType = contentItem.ContentType,
                                ContentPart = fieldDefinition.PartDefinition.Name,
                                ContentField = fieldDefinition.Name,
                                TermContentItemId = termContentItemId,
                            });
                        }
                    }

                    return results;
                });
        }
    }
}
