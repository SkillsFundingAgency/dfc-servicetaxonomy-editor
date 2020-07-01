using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Taxonomies.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Taxonomy
{
    public class TaxonomyPartGraphSyncer : IContentPartGraphSyncer
    {
        private readonly ITaxonomyPartEmbeddedContentItemsGraphSyncer _taxonomyPartEmbeddedContentItemsGraphSyncer;
        public string PartName => nameof(TaxonomyPart);

        private const string ContainerName = "Terms";
        private const string TermContentTypePropertyName = "TermContentType";

        public TaxonomyPartGraphSyncer(
            ITaxonomyPartEmbeddedContentItemsGraphSyncer taxonomyPartEmbeddedContentItemsGraphSyncer)
        {
            _taxonomyPartEmbeddedContentItemsGraphSyncer = taxonomyPartEmbeddedContentItemsGraphSyncer;
        }

        public async Task AddSyncComponents(JObject content,
            ContentItem contentItem,
            IContentManager contentManager,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            await _taxonomyPartEmbeddedContentItemsGraphSyncer.AddSyncComponents(
                (JArray?)content[ContainerName],
                contentManager,
                replaceRelationshipsCommand,
                graphSyncHelper);

            // useful if there are no terms yet?
            mergeNodeCommand.AddProperty<string>(TermContentTypePropertyName, content);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            ContentTypePartDefinition contentTypePartDefinition,
            IContentManager contentManager,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts,
            IValidateAndRepairGraph validateAndRepairGraph)
        {
            (bool validated, string failureReason) =
                await _taxonomyPartEmbeddedContentItemsGraphSyncer.ValidateSyncComponent(
                    (JArray?)content[ContainerName],
                    contentManager,
                    nodeWithOutgoingRelationships,
                    graphValidationHelper,
                    expectedRelationshipCounts,
                    validateAndRepairGraph);

            if (!validated)
                return (validated, failureReason);

            return graphValidationHelper.StringContentPropertyMatchesNodeProperty(
                TermContentTypePropertyName,
                content,
                TermContentTypePropertyName,
                nodeWithOutgoingRelationships.SourceNode);
        }
    }
}
