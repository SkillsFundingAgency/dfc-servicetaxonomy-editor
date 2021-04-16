using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using DFC.ServiceTaxonomy.Taxonomies.Models;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.Taxonomies.Settings;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    //todo: remove `The body of the content item.`
    //todo: what happens when page references term that then gets deleted from the taxonomy? think sync would be ok, validate's GetNodeId wouldn't find any elements
    // if that's a thing, how should we handle it?

    /// <remarks>
    /// We don't bother syncing TermParts:
    ///   Whilst Orchard Core has a TermPart, we don't need to sync it, as we already create a relationship
    ///   from taxonomy to term. Syncing the TermPart would only add a relationship in the other direction.
    /// Creating new terms from the taxonomy field itself:
    ///   The code handles syncing terms created when the user creates a new term from within the taxonomy field
    ///   in a consuming type, but as the term only has a DisplayText at that point, it can't be fetched from the content api.
    ///   Therefore, we disable adding terms from the taxonomy field, and force the user to add new terms through
    ///   the taxonomy item instead.
    ///   We could possibly generate a GraphSyncPart's details just in time, but atm we don't.
    /// </remarks>
    public class TaxonomyFieldGraphSyncer : IContentFieldGraphSyncer
    {
        private readonly IServiceProvider _serviceProvider;
        public string FieldTypeName => "TaxonomyField";

        private const string TagNames = "TagNames";
        private const string TaxonomyContentItemId = "TaxonomyContentItemId";
        private const string TermContentItemIds = "TermContentItemIds";
        private const string TermContentType = "TermContentType";
        private const string Terms = "Terms";
        private const string TaxonomyTermsNodePropertyName = "taxonomy_terms";

        public TaxonomyFieldGraphSyncer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task AddSyncComponents(JObject contentItemField, IGraphMergeContext context)
        {
            //todo: share code with contentpickerfield?

            //todo: better error message to user if taxonomy is missing
            //todo: check for null
            ContentItem? taxonomyContentItem = await GetTaxonomyContentItem(
                contentItemField, context.ContentItemVersion, context.ContentManager);

            JObject taxonomyPartContent = taxonomyContentItem!.Content[nameof(TaxonomyPart)];
            string? termContentType = taxonomyPartContent[TermContentType]?.Value<string>();
            if (!string.IsNullOrEmpty(termContentType))
            {
                string termRelationshipType = TermRelationshipType(termContentType);

                //todo requires 'picked' part has a graph sync part
                // add to docs & handle picked part not having graph sync part or throw exception

                JArray? contentItemIdsJArray = (JArray?)contentItemField[TermContentItemIds];
                if (contentItemIdsJArray == null || !contentItemIdsJArray.HasValues)
                    return; //todo:

                IEnumerable<string> contentItemIds = contentItemIdsJArray.Select(jtoken => jtoken.ToObject<string>()!);

                ISyncNameProvider relatedSyncNameProvider = _serviceProvider.GetSyncNameProvider(termContentType);

                var flattenedTermsContentItems = GetFlattenedTermsContentItems(taxonomyPartContent);

                IEnumerable<object> foundDestinationNodeIds = contentItemIds.Select(tid =>
                    GetNodeId(tid, flattenedTermsContentItems, relatedSyncNameProvider, context.ContentItemVersion)!);

                IEnumerable<string> destNodeLabels = await relatedSyncNameProvider.NodeLabels();

                context.ReplaceRelationshipsCommand.AddRelationshipsTo(
                    termRelationshipType,
                    null,
                    destNodeLabels,
                    relatedSyncNameProvider!.IdPropertyName(),
                    foundDestinationNodeIds.ToArray());

                // add relationship to taxonomy
                string taxonomyRelationshipType = TaxonomyRelationshipType(taxonomyContentItem);

                relatedSyncNameProvider.ContentType = taxonomyContentItem.ContentType;
                destNodeLabels = await relatedSyncNameProvider.NodeLabels();
                object taxonomyIdValue = relatedSyncNameProvider.GetNodeIdPropertyValue(
                    taxonomyContentItem.Content[nameof(GraphSyncPart)], context.ContentItemVersion);

                context.ReplaceRelationshipsCommand.AddRelationshipsTo(
                    taxonomyRelationshipType,
                    null,
                    destNodeLabels,
                    relatedSyncNameProvider!.IdPropertyName(),
                    taxonomyIdValue);
            }
            // add tagnames
            //using var _ = s  yncNameProvider.PushPropertyNameTransform(_taxonomyPropertyNameTransform);

            //todo: this is there sometimes, but not others (Tag editor is selected and/or non-unique?)
            // find out when it should be there: need to know to know how to validate it
            //context.MergeNodeCommand.AddArrayProperty<string>(TaxonomyTermsNodePropertyName, contentItemField, TagNames);

            //todo: need to store location as string, e.g. "/contact-us"
            // could alter flatten to also include parent and work backwards
            // or do a search, recording the path
        }

        public async Task AddSyncComponentsDetaching(IGraphMergeContext context)
        {
            var taxonomyFieldSettings = context.ContentPartFieldDefinition!.GetSettings<TaxonomyFieldSettings>();

            //todo: factor out common code
            //todo: check for null
            ContentItem? taxonomyContentItem =  await context.ContentItemVersion.GetContentItem(
                context.ContentManager,
                taxonomyFieldSettings.TaxonomyContentItemId);

            JObject taxonomyPartContent = taxonomyContentItem!.Content[nameof(TaxonomyPart)];
            string? termContentType = taxonomyPartContent[TermContentType]?.Value<string>();
            if (!string.IsNullOrEmpty(termContentType))
            {
                string termRelationshipType = TermRelationshipType(termContentType);

                //todo: split into stefull and stateless and put both in the context
                // then can stop stateful contenttype being reset
                IEnumerable<string> destNodeLabels = await context.SyncNameProvider.NodeLabels(termContentType);

                context.ReplaceRelationshipsCommand.RemoveAnyRelationshipsTo(
                    termRelationshipType,
                    destNodeLabels);


                string taxonomyRelationshipType = TaxonomyRelationshipType(taxonomyContentItem);

                destNodeLabels = await context.SyncNameProvider.NodeLabels(taxonomyContentItem.ContentType);

                context.ReplaceRelationshipsCommand.RemoveAnyRelationshipsTo(
                    taxonomyRelationshipType,
                    destNodeLabels);
            }
        }

        private static Dictionary<string, ContentItem> GetFlattenedTermsContentItems(JObject taxonomyPartContent)
        {
            IEnumerable<ContentItem> termsRootContentItems = GetTermsRootContentItems(taxonomyPartContent);

            return termsRootContentItems
                .Flatten(ci => ((JArray?)((JObject)ci.Content)["Terms"])
                    ?.ToObject<IEnumerable<ContentItem>>() ?? Enumerable.Empty<ContentItem>())
                .ToDictionary(ci => ci.ContentItemId, ci => ci.ContentItem);
        }

        private static IEnumerable<ContentItem> GetTermsRootContentItems(JObject taxonomyPartContent)
        {
            return ((JArray)taxonomyPartContent[Terms]!).ToObject<IEnumerable<ContentItem>>()!;
        }

        private object? GetNodeId(
            string termContentItemId,
            IDictionary<string, ContentItem> taxonomyTerms,
            ISyncNameProvider termSyncNameProvider,
            IContentItemVersion contentItemVersion)
        {
            ContentItem termContentItem = taxonomyTerms[termContentItemId];
            return termSyncNameProvider.GetNodeIdPropertyValue(
                (JObject)termContentItem.Content[nameof(GraphSyncPart)]!, contentItemVersion);
        }

        private async Task<ContentItem?> GetTaxonomyContentItem(
            JObject contentItemField,
            IContentItemVersion contentItemVersion,
            IContentManager contentManager)
        {
            string taxonomyContentItemId = contentItemField[TaxonomyContentItemId]?.ToObject<string>()!;
            //todo: null?

            //todo: need to really think this through/test it
            return await contentItemVersion.GetContentItem(contentManager, taxonomyContentItemId);
        }

        private string TermRelationshipType(string termContentType)
        {
            return $"has{termContentType}";
        }

        private string TaxonomyRelationshipType(ContentItem taxonomyContentItem)
        {
            string taxonomyName = taxonomyContentItem.DisplayText.Replace(" ", "");
            return $"has{taxonomyName}Taxonomy";
        }

        public async Task AddRelationship(JObject contentItemField, IDescribeRelationshipsContext parentContext)
        {
            //todo: check for null
            ContentItem? taxonomyContentItem = await GetTaxonomyContentItem(
                contentItemField,
                parentContext.ContentItemVersion,
                parentContext.ContentManager);

            JObject taxonomyPartContent = taxonomyContentItem!.Content[nameof(TaxonomyPart)];
            string? termContentType = taxonomyPartContent[TermContentType]?.Value<string>();
            if (!string.IsNullOrEmpty(termContentType))
            {
                string termRelationshipType = TermRelationshipType(termContentType);

                //todo: auto collect all taxonomy terms? or go through build relationships?

                const int maxDepthFromHere = 0;

                var sourceNodeLabels =
                    await parentContext.SyncNameProvider.NodeLabels(parentContext.ContentItem.ContentType);

                // gets auto-added to parent. better way though?
#pragma warning disable S1848
                new DescribeRelationshipsContext(
                    parentContext.SourceNodeIdPropertyName, parentContext.SourceNodeId, parentContext.SourceNodeLabels,
                    parentContext.ContentItem, maxDepthFromHere, parentContext.SyncNameProvider,
                    parentContext.ContentManager,
                    parentContext.ContentItemVersion, parentContext, parentContext.ServiceProvider)
                {
                    AvailableRelationships = new List<ContentItemRelationship>
                    {
                        new ContentItemRelationship(
                            sourceNodeLabels,
                            termRelationshipType,
                            await parentContext.SyncNameProvider.NodeLabels(termContentType))
                    }
                };

                string taxonomyRelationshipType = TaxonomyRelationshipType(taxonomyContentItem);

                new DescribeRelationshipsContext(
                    parentContext.SourceNodeIdPropertyName, parentContext.SourceNodeId, parentContext.SourceNodeLabels,
                    parentContext.ContentItem, maxDepthFromHere, parentContext.SyncNameProvider,
                    parentContext.ContentManager,
                    parentContext.ContentItemVersion, parentContext, parentContext.ServiceProvider)
                {
                    AvailableRelationships = new List<ContentItemRelationship>
                    {
                        new ContentItemRelationship(
                            sourceNodeLabels,
                            taxonomyRelationshipType,
                            await parentContext.SyncNameProvider.NodeLabels(taxonomyContentItem.ContentType))
                    }
                };
            }

#pragma warning restore S1848
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject contentItemField,
            IValidateAndRepairContext context)
        {
            //todo: check for null
            ContentItem? taxonomyContentItem = await GetTaxonomyContentItem(
                contentItemField, context.ContentItemVersion, context.ContentManager);

            var taxonomyPartContent = taxonomyContentItem!.Content[nameof(TaxonomyPart)];
            string termContentType = taxonomyPartContent[TermContentType];

            string termRelationshipType = TermRelationshipType(termContentType);

            IRelationship[] actualRelationships = context.NodeWithRelationships.OutgoingRelationships
                .Where(r => r.Type == termRelationshipType)
                .ToArray();

            var contentItemIds = (JArray)contentItemField[TermContentItemIds]!;
            if (contentItemIds.Count != actualRelationships.Length)
            {
                return (false, $"expecting {contentItemIds.Count} relationships of type {termRelationshipType} in graph, but found {actualRelationships.Length}");
            }

            ISyncNameProvider relatedSyncNameProvider = _serviceProvider.GetSyncNameProvider(termContentType);

            var flattenedTermsContentItems = GetFlattenedTermsContentItems(taxonomyPartContent);

            foreach (JToken item in contentItemIds)
            {
                string contentItemId = (string)item!;

                object? destinationId = GetNodeId(
                    contentItemId, flattenedTermsContentItems, relatedSyncNameProvider, context.ContentItemVersion)!;

                (bool validated, string failureReason) = context.GraphValidationHelper.ValidateOutgoingRelationship(
                    context.NodeWithRelationships,
                    termRelationshipType,
                    relatedSyncNameProvider!.IdPropertyName(),
                    destinationId);

                if (!validated)
                    return (false, failureReason);

                // keep a count of how many relationships of a type we expect to be in the graph
                context.ExpectedRelationshipCounts.IncreaseCount(termRelationshipType);
            }

            // return context.GraphValidationHelper.StringArrayContentPropertyMatchesNodeProperty(
            //     TagNames,
            //     contentItemField,
            //     TaxonomyTermsNodePropertyName,
            //     context.NodeWithOutgoingRelationships.SourceNode);

            return (true, "");
        }
    }
}
