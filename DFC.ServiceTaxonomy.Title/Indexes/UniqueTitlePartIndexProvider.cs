using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Title.Models;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.Title.Indexes
{
    public class UniqueTitlePartIndexProvider : ContentHandlerBase, IScopedIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HashSet<string> _partRemoved = new HashSet<string>();
        private IContentDefinitionManager? _contentDefinitionManager;

        public UniqueTitlePartIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override Task UpdatedAsync(UpdateContentContext context)
        {
            var part = context.ContentItem.As<UniqueTitlePart>();

            // Validate that the content definition contains this part, this prevents indexing parts
            // that have been removed from the type definition, but are still present in the elements.            
            if (part != null)
            {
                // Lazy initialization because of ISession cyclic dependency.
                _contentDefinitionManager ??= _serviceProvider.GetRequiredService<IContentDefinitionManager>();

                // Search for this part.
                var contentTypeDefinition =
                    _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
                if (!contentTypeDefinition.Parts.Any(ctpd => ctpd.Name == nameof(UniqueTitlePart)))
                {
                    context.ContentItem.Remove<UniqueTitlePart>();
                    _partRemoved.Add(context.ContentItem.ContentItemId);
                }
            }

            return Task.CompletedTask;
        }

        public string? CollectionName { get; set; }
        public Type ForType() => typeof(ContentItem);
        public void Describe(IDescriptor context) => Describe((DescribeContext<ContentItem>)context);

        public void Describe(DescribeContext<ContentItem> context)
        {
            context.For<UniqueTitlePartIndex>()
                .When(contentItem => contentItem.Has<UniqueTitlePart>() || _partRemoved.Contains(contentItem.ContentItemId))
                .Map(contentItem =>
                {
                    // Remove index records of soft deleted items.
                    if (!contentItem.Published && !contentItem.Latest)
                    {
#pragma warning disable CS8603 // Possible null reference return.
                        return null;
#pragma warning restore CS8603 // Possible null reference return.
                    }

                    var part = contentItem.As<UniqueTitlePart>();
                    if (part == null || String.IsNullOrEmpty(part.Title))
                    {
#pragma warning disable CS8603 // Possible null reference return.
                        return null;
#pragma warning restore CS8603 // Possible null reference return.
                    }

                    return new UniqueTitlePartIndex
                    {
                        Title = part.Title?.ToLowerInvariant(),
                        ContentItemId = contentItem.ContentItemId,
                        Latest = contentItem.Latest,
                        Published = contentItem.Published
                    };
                });
        }
    }
}
