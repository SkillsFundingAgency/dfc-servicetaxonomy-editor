using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public class DescribeContentItemHelper : IDescribeContentItemHelper
    {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly IEnumerable<IContentPartGraphSyncer> _contentPartGraphSyncers;
        private readonly IEnumerable<IContentFieldGraphSyncer> _contentFieldsGraphSyncers;
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly IServiceProvider _serviceProvider;

        public ContentItem? RootContentItem { get; private set; }

        public DescribeContentItemHelper(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IGraphSyncHelper graphSyncHelper,
            IEnumerable<IContentPartGraphSyncer> contentPartGraphSyncers,
            IEnumerable<IContentFieldGraphSyncer> contentFieldsGraphSyncers,
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion,
            IServiceProvider serviceProvider)
        {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _graphSyncHelper = graphSyncHelper;
            _contentPartGraphSyncers = contentPartGraphSyncers;
            _contentFieldsGraphSyncers = contentFieldsGraphSyncers;
            _publishedContentItemVersion = publishedContentItemVersion;
            _previewContentItemVersion = previewContentItemVersion;
            _serviceProvider = serviceProvider;
        }

        public void SetRootContentItem(ContentItem contentItem)
        {
            RootContentItem = contentItem;
        }

        public async Task<IEnumerable<ContentItemRelationship>> GetRelationships(IDescribeRelationshipsContext context, List<ContentItemRelationship> currentList)
        {
            if (context.AvailableRelationships != null)
            {
                currentList.AddRange(context.AvailableRelationships);
            }

            foreach(var childContext in context.ChildContexts)
            {
                await GetRelationships((IDescribeRelationshipsContext)childContext, currentList);
            }

            return currentList;
        }

        public async Task BuildRelationships(ContentItem contentItem, IDescribeRelationshipsContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            _graphSyncHelper.ContentType = contentItem.ContentType;

            var itemContext = contentItem == RootContentItem ? context : new DescribeRelationshipsContext(contentItem, _graphSyncHelper, _contentManager, _publishedContentItemVersion, context, _serviceProvider);
            foreach (var part in contentTypeDefinition.Parts)
            {
                var partSyncer = _contentPartGraphSyncers.FirstOrDefault(x => x.PartName == part.Name);

                if (partSyncer != null)
                {
                    await partSyncer.AddRelationship(itemContext);
                }

                foreach (var relationshipField in part.PartDefinition.Fields)
                {
                    //var fieldContext = new DescribeRelationshipsContext(contentItem, _graphSyncHelper, _contentManager, _publishedContentItemVersion, partContext, _serviceProvider);
                    itemContext.SetContentPartFieldDefinition(relationshipField);
                    itemContext.SetContentField((JObject)contentItem.Content[relationshipField.PartDefinition.Name][relationshipField.Name]);
                    var fieldSyncer = _contentFieldsGraphSyncers.FirstOrDefault(x => x.FieldTypeName == relationshipField.FieldDefinition.Name);

                    var contentItemIds =
                           (JArray)contentItem.Content[relationshipField.PartDefinition.Name][relationshipField.Name]
                               .ContentItemIds;

                    if (contentItemIds != null)
                    {
                        foreach (var relatedContentItemId in contentItemIds)
                        {
                            await BuildRelationships(await _contentManager.GetAsync(relatedContentItemId.ToString()), itemContext);
                        }
                    }

                    if (fieldSyncer != null)
                    {
                        await fieldSyncer.AddRelationship(itemContext);
                    }

                }
            }

            if (contentItem != RootContentItem)
            {
                context.AddChildContext(itemContext);
            }
        }
    }
}
