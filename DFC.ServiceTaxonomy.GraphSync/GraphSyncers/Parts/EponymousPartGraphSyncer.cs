using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CustomFields.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
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

        public bool CanSync(string contentType, ContentPartDefinition contentPartDefinition)
        {
            return contentPartDefinition.Name == contentType
                || contentPartDefinition.Fields.Any(f => _groupingFields.Contains(f.FieldDefinition.Name));
        }

        public async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            await _contentFieldsGraphSyncer.AddSyncComponents(content, context);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            return await _contentFieldsGraphSyncer.ValidateSyncComponent(
                content, context);
        }
    }
}
