using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using Microsoft.Extensions.DependencyInjection;
//using MoreLinq;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Taxonomies.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    //todo: remove `The body of the content item.`

    public static class Xyz
    {
        public static IEnumerable<T> Flatten<T>(
            this IEnumerable<T> e
            , Func<T, IEnumerable<T>> f
        ) => e.SelectMany(c => f(c).Flatten(f)).Concat(e);
    }

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
                contentItemField, context.ContentManager, context.ContentItemVersion);

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

            //todo: handle missing graphsynchelper. extract into GetNodeId method
            JArray taxonomyTermsContent = GetTerms(taxonomyPartContent);

            //var contentItems = taxonomyTermsContent.Values<ContentItem>();
            var contentItems2 = taxonomyTermsContent.ToObject<IEnumerable<ContentItem>>();

            //var abc = contentItems.Flatten(ci => ((JArray)((ContentItem)ci).Content)["Terms"]!.Values<ContentItem>());
            //            var abc2 = contentItems2.Flatten(ci => ((JArray?)((JObject)((ContentItem)ci).Content)["Terms"])?.ToObject<IEnumerable<ContentItem>>());

            var abc2 = contentItems2!.Flatten(ci => ((JArray?)((JObject)ci.Content)["Terms"])?.ToObject<IEnumerable<ContentItem>>() ?? Enumerable.Empty<ContentItem>());


#pragma warning disable S1481

            // could only get morelinq's flatten to produce the leaf nodes :(

            //var cis = abc.Cast<ContentItem>();
            //var cis2 = abc2.Cast<ContentItem>();

            //var abc = MoreEnumerable.TraverseDepthFirst(contentItems)

            //var xyz = taxonomyTermsContent.Flatten(token => (JArray)((JObject)token)["Terms"]!);
            //var xyz = MoreEnumerable.TraverseDepthFirst(taxonomyTermsContent, )
            //            var abc = (IEnumerable<JObject>)xyz;

            IEnumerable<object> foundDestinationNodeIds = contentItemIds.Select(tid =>
                GetNodeId(tid, abc2, relatedGraphSyncHelper)!);

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
            object taxonomyIdValue = relatedGraphSyncHelper.GetIdPropertyValue(taxonomyContentItem.Content[nameof(GraphSyncPart)]);

            context.ReplaceRelationshipsCommand.AddRelationshipsTo(
                taxonomyRelationshipType,
                null,
                destNodeLabels,
                relatedGraphSyncHelper!.IdPropertyName(),
                taxonomyIdValue);

            // add tagnames
            //using var _ = graphSyncHelper.PushPropertyNameTransform(_taxonomyPropertyNameTransform);

            context.MergeNodeCommand.AddArrayProperty<string>(TaxonomyTermsNodePropertyName, contentItemField, TagNames);
        }

        private static JArray GetTerms(JObject taxonomyPartContent)
        {
            JArray taxonomyTermsContent = (JArray) taxonomyPartContent[Terms]!;
            return taxonomyTermsContent;
        }

        // private object? GetNodeId(string termContentItemId, JArray taxonomyTermsContent, IGraphSyncHelper termGraphSyncHelper)
        // {
        //     var xyz = taxonomyTermsContent.Flatten(token => (JArray)((JObject)token)["Terms"]!);
        //
        //     JObject? termContentItem;
        //
        //     do
        //     {
        //         termContentItem = (JObject)taxonomyTermsContent.FirstOrDefault(token =>
        //             token["ContentItemId"]?.Value<string>() == termContentItemId);
        //
        //         if (termContentItem == null)
        //         {
        //             taxonomyTermsContent
        //         }
        //
        //     } while (termContentItem == null);
        //
        //     return termGraphSyncHelper.GetIdPropertyValue((JObject)termContentItem[nameof(GraphSyncPart)]!);
        // }

        // ToDictionary??
        private object? GetNodeId(string termContentItemId, JArray taxonomyTermsContent, IGraphSyncHelper termGraphSyncHelper)
        {
            JObject termContentItem = (JObject)taxonomyTermsContent.First(token => token["ContentItemId"]?.Value<string>() == termContentItemId);
            return termGraphSyncHelper.GetIdPropertyValue((JObject)termContentItem[nameof(GraphSyncPart)]!);
        }

        private object? GetNodeId(string termContentItemId, IEnumerable<JObject> taxonomyTermsContent, IGraphSyncHelper termGraphSyncHelper)
        {
            JObject termContentItem = taxonomyTermsContent.First(token => token["ContentItemId"]?.Value<string>() == termContentItemId);
            return termGraphSyncHelper.GetIdPropertyValue((JObject)termContentItem[nameof(GraphSyncPart)]!);
        }

        private object? GetNodeId(string termContentItemId, IEnumerable<ContentItem> taxonomyTermsContent, IGraphSyncHelper termGraphSyncHelper)
        {
            ContentItem termContentItem = taxonomyTermsContent.First(ci => ci.ContentItemId == termContentItemId);
            return termGraphSyncHelper.GetIdPropertyValue((JObject)termContentItem.Content[nameof(GraphSyncPart)]!);
        }

        private async Task<ContentItem> GetTaxonomyContentItem(
            JObject contentItemField,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion)
        {
            string taxonomyContentItemId = contentItemField[TaxonomyContentItemId]?.ToObject<string>()!;
            //todo: null?

            //todo: need to really think this through/test it
            return await contentItemVersion.GetContentItemAsync(contentManager, taxonomyContentItemId);
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

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject contentItemField,
            IValidateAndRepairContext context)
        {
            ContentItem taxonomyContentItem = await GetTaxonomyContentItem(
                contentItemField, context.ContentManager, context.ContentItemVersion);

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

            JArray taxonomyTermsContent = (JArray)taxonomyPartContent[Terms];

            foreach (JToken item in contentItemIds)
            {
                string contentItemId = (string)item!;

                object? destinationId = GetNodeId(contentItemId, taxonomyTermsContent, relatedGraphSyncHelper)!;

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
