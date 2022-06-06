using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DFC.ServiceTaxonomy.UnpublishLater.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data;
using YesSql.Indexes;







namespace DFC.ServiceTaxonomy.UnpublishLater.Indexes
{
    public class UnpublishLaterPartIndex : MapIndex
    {
        public DateTime? ScheduledUnpublishUtc { get; set; }
    }

    public class UnpublishLaterPartIndexProvider : ContentHandlerBase, IIndexProvider, IScopedIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HashSet<string> _partRemoved = new HashSet<string>();
        private IContentDefinitionManager _contentDefinitionManager;


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public UnpublishLaterPartIndexProvider(IServiceProvider serviceProvider)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _serviceProvider = serviceProvider;
        }

        public override Task UpdatedAsync(UpdateContentContext context)
        {
            var part = context.ContentItem.As<UnpublishLaterPart>();

            // Validate that the content definition contains this part, this prevents indexing parts
            // that have been removed from the type definition, but are still present in the elements.            
            if (part != null)
            {
                // Lazy initialization because of ISession cyclic dependency.
                _contentDefinitionManager ??= _serviceProvider.GetRequiredService<IContentDefinitionManager>();

                // Search for this part.
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
                if (!contentTypeDefinition.Parts.Any(ctpd => ctpd.Name == nameof(UnpublishLaterPart)))
                {
                    context.ContentItem.Remove<UnpublishLaterPart>();
                    _partRemoved.Add(context.ContentItem.ContentItemId);
                }
            }

            return Task.CompletedTask;
        }

        public string CollectionName { get; set; }
        public Type ForType() => typeof(ContentItem);
        public void Describe(IDescriptor context) => Describe((DescribeContext<ContentItem>)context);

        public void Describe(DescribeContext<ContentItem> context)
        {
            context.For<UnpublishLaterPartIndex>()
                .When(contentItem => contentItem.Has<UnpublishLaterPart>() || _partRemoved.Contains(contentItem.ContentItemId))
                .Map(contentItem =>
                {
                    // Remove index records of items that are already published or not the latest version.
                    if (!contentItem.Published || !contentItem.Latest)
                    {
#pragma warning disable CS8603 // Possible null reference return.
                        return null;
#pragma warning restore CS8603 // Possible null reference return.
                    }

                    var part = contentItem.As<UnpublishLaterPart>();
                    if (part == null || !part.ScheduledUnpublishUtc.HasValue)
                    {
#pragma warning disable CS8603 // Possible null reference return.
                        return null;
#pragma warning restore CS8603 // Possible null reference return.
                    }

                    return new UnpublishLaterPartIndex
                    {
                        ScheduledUnpublishUtc = part.ScheduledUnpublishUtc,
                    };
                });
        }
    }

}
