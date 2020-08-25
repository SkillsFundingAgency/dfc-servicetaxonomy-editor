using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using Newtonsoft.Json.Linq;
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

        public override async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            context.MergeNodeCommand.AddProperty<string>(TermContentTypePropertyName, content);

            _embeddedContentItemsGraphSyncer.IsNonLeafEmbeddedTerm = false;
            await _embeddedContentItemsGraphSyncer.AddSyncComponents((JArray?)content[ContainerName], context);
        }

        public async Task AddSyncComponentsForNonLeafEmbeddedTerm(JObject content, IGraphMergeContext context)
        {
            _embeddedContentItemsGraphSyncer.IsNonLeafEmbeddedTerm = true;
            await _embeddedContentItemsGraphSyncer.AddSyncComponents((JArray?)content[ContainerName], context);
        }

        public override async Task DeleteComponents(JObject content, IGraphDeleteContext context)
        {
            _embeddedContentItemsGraphSyncer.IsNonLeafEmbeddedTerm = false;
            await _embeddedContentItemsGraphSyncer.DeleteComponents((JArray?)content[ContainerName], context);
        }

        public async Task DeleteComponentsForNonLeafEmbeddedTerm(JObject content, IGraphDeleteContext context)
        {
            _embeddedContentItemsGraphSyncer.IsNonLeafEmbeddedTerm = true;
            await _embeddedContentItemsGraphSyncer.DeleteComponents((JArray?)content[ContainerName], context);
        }

        public override async Task MutateOnClone(JObject content, ICloneContext context)
        {
            await _taxonomyPartEmbeddedContentItemsGraphSyncer.MutateOnClone((JArray?)content[ContainerName], context);
        }

        //todo: we now need to validate any 2 way incoming relationships we created
        public override async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            (bool validated, string failureReason) =
                await _embeddedContentItemsGraphSyncer.ValidateSyncComponent(
                    (JArray?)content[ContainerName], context);

            if (!validated)
                return (validated, failureReason);

            return context.GraphValidationHelper.StringContentPropertyMatchesNodeProperty(
                TermContentTypePropertyName,
                content,
                TermContentTypePropertyName,
                context.NodeWithOutgoingRelationships.SourceNode);
        }
    }
}
