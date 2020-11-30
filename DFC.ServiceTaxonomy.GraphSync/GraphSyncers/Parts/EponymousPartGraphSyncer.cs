using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CustomFields.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class EponymousPartGraphSyncer : ContentPartGraphSyncer
    {
        private readonly IContentFieldsGraphSyncer _contentFieldsGraphSyncer;

        public EponymousPartGraphSyncer(IContentFieldsGraphSyncer contentFieldsGraphSyncer)
        {
            _contentFieldsGraphSyncer = contentFieldsGraphSyncer;
        }

        public override string PartName => "EponymousPart";

        private static readonly List<string> _groupingFields = new List<string>
        {
            nameof(TabField),
            nameof(AccordionField)
        };

        //todo: sync custom parts with no grouping field : have eponymous as syncer of last resort? have dummy syncers for non-user parts that cause issues?
        public override bool CanSync(string contentType, ContentPartDefinition contentPartDefinition)
        {
            return contentPartDefinition.Name == contentType
                || contentPartDefinition.Fields.Any(f => _groupingFields.Contains(f.FieldDefinition.Name));
        }

        public override async Task AllowSync(JObject content, IGraphMergeContext context, IAllowSync allowSync)
        {
            await _contentFieldsGraphSyncer.AllowSync(content, context, allowSync);
        }

        public override async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            await _contentFieldsGraphSyncer.AddSyncComponents(content, context);
        }

        public override async Task AddRelationship(JObject content, IDescribeRelationshipsContext context)
        {
            await _contentFieldsGraphSyncer.AddRelationship(content, context);
        }

        public override async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            return await _contentFieldsGraphSyncer.ValidateSyncComponent(
                content, context);
        }
    }
}
