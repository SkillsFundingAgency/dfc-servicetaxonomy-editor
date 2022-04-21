using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.EmbeddedContentItemsDataSyncer;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.DataSync.Extensions;
using DFC.ServiceTaxonomy.Taxonomies.Models;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts.Taxonomy
{
    public class TaxonomyPartDataSyncer : EmbeddingContentPartDataSyncer, ITaxonomyPartDataSyncer
    {
        protected readonly new ITaxonomyPartEmbeddedContentItemsDataSyncer _embeddedContentItemsDataSyncer;

        public override string PartName => nameof(TaxonomyPart);
        protected override string ContainerName => "Terms";

        internal const string TermContentTypePropertyName = "TermContentType";

        public TaxonomyPartDataSyncer(
            ITaxonomyPartEmbeddedContentItemsDataSyncer taxonomyPartEmbeddedContentItemsDataSyncer)
        : base(taxonomyPartEmbeddedContentItemsDataSyncer)
        {
            _embeddedContentItemsDataSyncer = taxonomyPartEmbeddedContentItemsDataSyncer;
        }

        public override Task AddSyncComponents(JObject content, IDataMergeContext context)
        {
            context.MergeNodeCommand.AddProperty<string>(TermContentTypePropertyName, content);

            _embeddedContentItemsDataSyncer.IsNonLeafEmbeddedTerm = false;
            return _embeddedContentItemsDataSyncer.AddSyncComponents((JArray?)content[ContainerName], context);
        }

        public Task AddSyncComponentsForNonLeafEmbeddedTerm(JObject content, IDataMergeContext context)
        {
            _embeddedContentItemsDataSyncer.IsNonLeafEmbeddedTerm = true;
            return _embeddedContentItemsDataSyncer.AddSyncComponents((JArray?)content[ContainerName], context);
        }

        public override Task DeleteComponents(JObject content, IDataDeleteContext context)
        {
            _embeddedContentItemsDataSyncer.IsNonLeafEmbeddedTerm = false;
            return _embeddedContentItemsDataSyncer.DeleteComponents((JArray?)content[ContainerName], context);
        }

        public Task DeleteComponentsForNonLeafEmbeddedTerm(JObject content, IDataDeleteContext context)
        {
            _embeddedContentItemsDataSyncer.IsNonLeafEmbeddedTerm = true;
            return _embeddedContentItemsDataSyncer.DeleteComponents((JArray?)content[ContainerName], context);
        }

        //todo: we now need to validate any 2 way incoming relationships we created
        public override Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            _embeddedContentItemsDataSyncer.IsNonLeafEmbeddedTerm = false;
            return ValidateSyncComponentImplementation(content, context);
        }

        public Task<(bool validated, string failureReason)> ValidateSyncComponentForNonLeafEmbeddedTerm(
            JObject content,
            IValidateAndRepairContext context)
        {
            _embeddedContentItemsDataSyncer.IsNonLeafEmbeddedTerm = true;
            return ValidateSyncComponentImplementation(content, context);
        }

        private async Task<(bool validated, string failureReason)> ValidateSyncComponentImplementation(
            JObject content,
            IValidateAndRepairContext context)
        {
            (bool validated, string failureReason) =
                await _embeddedContentItemsDataSyncer.ValidateSyncComponent(
                    (JArray?)content[ContainerName], context);

            if (!validated)
                return (validated, failureReason);

            return context.DataSyncValidationHelper.StringContentPropertyMatchesNodeProperty(
                TermContentTypePropertyName,
                content,
                TermContentTypePropertyName,
                context.NodeWithRelationships.SourceNode!);
        }
    }
}
