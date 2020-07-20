using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Taxonomies.Models;

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

        public TaxonomyFieldGraphSyncer(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task AddSyncComponents(JObject contentItemField, IGraphMergeContext context)
        {
            //todo: share code with contentpickerfield?

            ContentItem taxonomyContentItem = await GetTaxonomyContentItem(
                contentItemField, context.ContentItemVersion, context.ContentManager);

            JObject taxonomyPartContent = taxonomyContentItem.Content[nameof(TaxonomyPart)];
            string termContentType = taxonomyPartContent[TermContentType]!.Value<string>();

            string termRelationshipType = TermRelationshipType(termContentType);

            //todo requires 'picked' part has a graph sync part
            // add to docs & handle picked part not having graph sync part or throw exception

            JArray? contentItemIdsJArray = (JArray?)contentItemField[TermContentItemIds];
            if (contentItemIdsJArray == null || !contentItemIdsJArray.HasValues)
                return; //todo:

            IEnumerable<string> contentItemIds = contentItemIdsJArray.Select(jtoken => jtoken.ToObject<string>()!);

            IGraphSyncHelper relatedGraphSyncHelper = _serviceProvider.GetRequiredService<IGraphSyncHelper>();
            relatedGraphSyncHelper.ContentType = termContentType;

            var flattenedTermsContentItems = GetFlattenedTermsContentItems(taxonomyPartContent);

            IEnumerable<object> foundDestinationNodeIds = contentItemIds.Select(tid =>
                GetNodeId(tid, flattenedTermsContentItems, relatedGraphSyncHelper, context.ContentItemVersion)!);

            IEnumerable<string> destNodeLabels = await relatedGraphSyncHelper.NodeLabels();

            context.ReplaceRelationshipsCommand.AddRelationshipsTo(
                termRelationshipType,
                null,
                destNodeLabels,
                relatedGraphSyncHelper!.IdPropertyName(),
                foundDestinationNodeIds.ToArray());

            // add relationship to taxonomy
            string taxonomyRelationshipType = TaxonomyRelationshipType(taxonomyContentItem);

            relatedGraphSyncHelper.ContentType = taxonomyContentItem.ContentType;
            destNodeLabels = await relatedGraphSyncHelper.NodeLabels();
            object taxonomyIdValue = relatedGraphSyncHelper.GetIdPropertyValue(
                taxonomyContentItem.Content[nameof(GraphSyncPart)], context.ContentItemVersion);

            context.ReplaceRelationshipsCommand.AddRelationshipsTo(
                taxonomyRelationshipType,
                null,
                destNodeLabels,
                relatedGraphSyncHelper!.IdPropertyName(),
                taxonomyIdValue);

            // add tagnames
            //using var _ = graphSyncHelper.PushPropertyNameTransform(_taxonomyPropertyNameTransform);

            //todo: this is there sometimes, but not others (Tag editor is selected and/or non-unique?)
            context.MergeNodeCommand.AddArrayProperty<string>(TaxonomyTermsNodePropertyName, contentItemField, TagNames);

            //todo: need to store location as string, e.g. "/contact-us"
            // could alter flatten to also include parent and work backwards
            // or do a search, recording the path
        }

        private static Dictionary<string, ContentItem> GetFlattenedTermsContentItems(JObject taxonomyPartContent)
        {
            IEnumerable<ContentItem> termsRootContentItems = GetTermsRootContentItems(taxonomyPartContent);

            return termsRootContentItems
                .Flatten(ci => ((JArray?) ((JObject) ci.Content)["Terms"])
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
            IGraphSyncHelper termGraphSyncHelper,
            IContentItemVersion contentItemVersion)
        {
            ContentItem termContentItem = taxonomyTerms[termContentItemId];
            return termGraphSyncHelper.GetIdPropertyValue(
                (JObject)termContentItem.Content[nameof(GraphSyncPart)]!, contentItemVersion);
        }

        private async Task<ContentItem> GetTaxonomyContentItem(
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

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject contentItemField,
            IValidateAndRepairContext context)
        {
            ContentItem taxonomyContentItem = await GetTaxonomyContentItem(
                contentItemField, context.ContentItemVersion, context.ContentManager);

            var taxonomyPartContent = taxonomyContentItem.Content[nameof(TaxonomyPart)];
            string termContentType = taxonomyPartContent[TermContentType];

            string termRelationshipType = TermRelationshipType(termContentType);

            IOutgoingRelationship[] actualRelationships = context.NodeWithOutgoingRelationships.OutgoingRelationships
                .Where(r => r.Relationship.Type == termRelationshipType)
                .ToArray();

            var contentItemIds = (JArray)contentItemField[TermContentItemIds]!;
            if (contentItemIds.Count != actualRelationships.Length)
            {
                return (false, $"expecting {contentItemIds.Count} relationships of type {termRelationshipType} in graph, but found {actualRelationships.Length}");
            }

            IGraphSyncHelper relatedGraphSyncHelper = _serviceProvider.GetRequiredService<IGraphSyncHelper>();
            relatedGraphSyncHelper.ContentType = termContentType;

            var flattenedTermsContentItems = GetFlattenedTermsContentItems(taxonomyPartContent);

            foreach (JToken item in contentItemIds)
            {
                string contentItemId = (string)item!;

                object? destinationId = GetNodeId(
                    contentItemId, flattenedTermsContentItems, relatedGraphSyncHelper, context.ContentItemVersion)!;

                (bool validated, string failureReason) = context.GraphValidationHelper.ValidateOutgoingRelationship(
                    context.NodeWithOutgoingRelationships,
                    termRelationshipType,
                    relatedGraphSyncHelper!.IdPropertyName(),
                    destinationId);

                if (!validated)
                    return (false, failureReason);

                // keep a count of how many relationships of a type we expect to be in the graph
                context.ExpectedRelationshipCounts.IncreaseCount(termRelationshipType);
            }

            return context.GraphValidationHelper.StringArrayContentPropertyMatchesNodeProperty(
                TagNames,
                contentItemField,
                TaxonomyTermsNodePropertyName,
                context.NodeWithOutgoingRelationships.SourceNode);
        }
    }
}
