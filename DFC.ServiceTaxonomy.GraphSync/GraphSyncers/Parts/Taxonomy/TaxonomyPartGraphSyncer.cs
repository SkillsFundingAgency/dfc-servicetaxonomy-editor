using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.Taxonomies.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Taxonomy
{
    public class TaxonomyPartGraphSyncer : EmbeddingContentPartGraphSyncer, ITaxonomyPartGraphSyncer
    {
        protected readonly new ITaxonomyPartEmbeddedContentItemsGraphSyncer _embeddedContentItemsGraphSyncer;

        public override string PartName => nameof(TaxonomyPart);
        protected override string ContainerName => "Terms";

        internal const string TermContentTypePropertyName = "TermContentType";

        public TaxonomyPartGraphSyncer(
            ITaxonomyPartEmbeddedContentItemsGraphSyncer taxonomyPartEmbeddedContentItemsGraphSyncer)
        : base(taxonomyPartEmbeddedContentItemsGraphSyncer)
        {
            _embeddedContentItemsGraphSyncer = taxonomyPartEmbeddedContentItemsGraphSyncer;
        }

        public override Task AddSyncComponents(JsonObject content, IGraphMergeContext context)
        {
            context.MergeNodeCommand.AddProperty<string>(TermContentTypePropertyName, content);

            _embeddedContentItemsGraphSyncer.IsNonLeafEmbeddedTerm = false;
            return _embeddedContentItemsGraphSyncer.AddSyncComponents((JsonArray?)content[ContainerName], context);
        }

        public Task AddSyncComponentsForNonLeafEmbeddedTerm(JsonObject content, IGraphMergeContext context)
        {
            _embeddedContentItemsGraphSyncer.IsNonLeafEmbeddedTerm = true;
            return _embeddedContentItemsGraphSyncer.AddSyncComponents((JsonArray?)content[ContainerName], context);
        }

        public override Task DeleteComponents(JsonObject content, IGraphDeleteContext context)
        {
            _embeddedContentItemsGraphSyncer.IsNonLeafEmbeddedTerm = false;
            return _embeddedContentItemsGraphSyncer.DeleteComponents((JsonArray?)content[ContainerName], context);
        }

        public Task DeleteComponentsForNonLeafEmbeddedTerm(JsonObject content, IGraphDeleteContext context)
        {
            _embeddedContentItemsGraphSyncer.IsNonLeafEmbeddedTerm = true;
            return _embeddedContentItemsGraphSyncer.DeleteComponents((JsonArray?)content[ContainerName], context);
        }

        //todo: we now need to validate any 2 way incoming relationships we created
        public override Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JsonObject content,
            IValidateAndRepairContext context)
        {
            _embeddedContentItemsGraphSyncer.IsNonLeafEmbeddedTerm = false;
            return ValidateSyncComponentImplementation(content, context);
        }

        public Task<(bool validated, string failureReason)> ValidateSyncComponentForNonLeafEmbeddedTerm(
            JsonObject content,
            IValidateAndRepairContext context)
        {
            _embeddedContentItemsGraphSyncer.IsNonLeafEmbeddedTerm = true;
            return ValidateSyncComponentImplementation(content, context);
        }

        private async Task<(bool validated, string failureReason)> ValidateSyncComponentImplementation(
            JsonObject content,
            IValidateAndRepairContext context)
        {
            (bool validated, string failureReason) =
                await _embeddedContentItemsGraphSyncer.ValidateSyncComponent(
                    (JsonArray?)content[ContainerName], context);

            if (!validated)
                return (validated, failureReason);

            return context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                TermContentTypePropertyName,
                content,
                TermContentTypePropertyName,
                context.NodeWithRelationships.SourceNode!);
        }
    }
}
