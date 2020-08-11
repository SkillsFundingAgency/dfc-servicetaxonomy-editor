using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CustomFields.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class EponymousPartGraphSyncer : IContentPartGraphSyncer
    {
        private readonly IContentFieldsGraphSyncer _contentFieldsGraphSyncer;

        public EponymousPartGraphSyncer(IContentFieldsGraphSyncer contentFieldsGraphSyncer)
        {
            _contentFieldsGraphSyncer = contentFieldsGraphSyncer;
        }

        public string PartName => "EponymousPart";

        private static readonly List<string> _groupingFields = new List<string>
        {
            nameof(TabField),
            nameof(AccordionField)
        };

        //todo: sync custom parts with no grouping field : have eponymous as syncer of last resort? have dummy syncers for non-user parts that cause issues?
        public bool CanSync(string contentType, ContentPartDefinition contentPartDefinition)
        {
            return contentPartDefinition.Name == contentType
                || contentPartDefinition.Fields.Any(f => _groupingFields.Contains(f.FieldDefinition.Name));
        }

        public async Task AllowSync(JObject content, IGraphMergeContext context, IAllowSyncResult allowSyncResult)
        {
            await _contentFieldsGraphSyncer.AllowSync(content, context, allowSyncResult);
        }

        public async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            await _contentFieldsGraphSyncer.AddSyncComponents(content, context);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            return await _contentFieldsGraphSyncer.ValidateSyncComponent(
                content, context);
        }
    }
}
