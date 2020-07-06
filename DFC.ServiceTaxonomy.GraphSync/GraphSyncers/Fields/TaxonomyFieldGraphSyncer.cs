using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Taxonomies.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    //todo: remove `The body of the content item.`

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
        private readonly IContentManager _contentManager;
        private readonly IServiceProvider _serviceProvider;
        public string FieldTypeName => "TaxonomyField";

        private const string TagNames = "TagNames";
        private const string TaxonomyContentItemId = "TaxonomyContentItemId";
        private const string TermContentItemIds = "TermContentItemIds";
        private const string TermContentType = "TermContentType";
        private const string Terms = "Terms";
        private const string TaxonomyTermsNodePropertyName = "taxonomy_terms";

        public TaxonomyFieldGraphSyncer(
            IContentManager contentManager,
            IServiceProvider serviceProvider)
        {
            _contentManager = contentManager;
            _serviceProvider = serviceProvider;
        }

        public async Task AddSyncComponents(
            JObject contentItemField,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            IContentPartFieldDefinition contentPartFieldDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            //todo: share code with contentpickerfield?

            ContentItem taxonomyContentItem = await GetTaxonomyContentItem(contentItemField);
            var taxonomyPartContent = taxonomyContentItem.Content[nameof(TaxonomyPart)];
            string termContentType = taxonomyPartContent[TermContentType];

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
            JArray taxonomyTermsContent = (JArray)taxonomyPartContent[Terms];
            IEnumerable<object> foundDestinationNodeIds = contentItemIds.Select(tid =>
                GetNodeId(tid, taxonomyTermsContent, relatedGraphSyncHelper)!);

            IEnumerable<string> destNodeLabels = await relatedGraphSyncHelper.NodeLabels();

            replaceRelationshipsCommand.AddRelationshipsTo(
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

            replaceRelationshipsCommand.AddRelationshipsTo(
                taxonomyRelationshipType,
                null,
                destNodeLabels,
                relatedGraphSyncHelper!.IdPropertyName(),
                taxonomyIdValue);

            // add tagnames
            //using var _ = graphSyncHelper.PushPropertyNameTransform(_taxonomyPropertyNameTransform);

            mergeNodeCommand.AddArrayProperty<string>(TaxonomyTermsNodePropertyName, contentItemField, TagNames);
        }

        private object? GetNodeId(string termContentItemId, JArray taxonomyTermsContent, IGraphSyncHelper termGraphSyncHelper)
        {
            JObject termContentItem = (JObject)taxonomyTermsContent.First(token => token["ContentItemId"]?.Value<string>() == termContentItemId);
            return termGraphSyncHelper.GetIdPropertyValue((JObject)termContentItem[nameof(GraphSyncPart)]!);
        }

        private async Task<ContentItem> GetTaxonomyContentItem(JObject contentItemField)
        {
            string taxonomyContentItemId = contentItemField[TaxonomyContentItemId]?.ToObject<string>()!;
            //todo: null?

            return await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.Published);
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
            IContentPartFieldDefinition contentPartFieldDefinition,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts)
        {
            ContentItem taxonomyContentItem = await GetTaxonomyContentItem(contentItemField);
            var taxonomyPartContent = taxonomyContentItem.Content[nameof(TaxonomyPart)];
            string termContentType = taxonomyPartContent[TermContentType];

            string termRelationshipType = TermRelationshipType(termContentType);

            IOutgoingRelationship[] actualRelationships = nodeWithOutgoingRelationships.OutgoingRelationships
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

                (bool validated, string failureReason) = graphValidationHelper.ValidateOutgoingRelationship(
                    nodeWithOutgoingRelationships,
                    termRelationshipType,
                    relatedGraphSyncHelper!.IdPropertyName(),
                    destinationId);

                if (!validated)
                    return (false, failureReason);

                // keep a count of how many relationships of a type we expect to be in the graph
                expectedRelationshipCounts.IncreaseCount(termRelationshipType);
            }

            return graphValidationHelper.StringArrayContentPropertyMatchesNodeProperty(
                TagNames,
                contentItemField,
                TaxonomyTermsNodePropertyName,
                nodeWithOutgoingRelationships.SourceNode);
        }
    }
}
