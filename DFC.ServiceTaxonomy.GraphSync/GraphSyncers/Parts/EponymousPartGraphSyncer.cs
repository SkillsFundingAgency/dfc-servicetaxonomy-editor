using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CustomFields.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
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

        public bool CanHandle(string contentType, ContentPartDefinition contentPartDefinition)
        {
            return contentPartDefinition.Name == contentType
                || contentPartDefinition.Fields.Any(f => _groupingFields.Contains(f.FieldDefinition.Name));
        }

        public async Task AddSyncComponents(JObject content,
            ContentItem contentItem,
            IContentManager contentManager,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            await _contentFieldsGraphSyncer.AddSyncComponents(
                content,
                contentManager,
                mergeNodeCommand,
                replaceRelationshipsCommand,
                contentTypePartDefinition,
                graphSyncHelper);
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
            return await _contentFieldsGraphSyncer.ValidateSyncComponent(
                content,
                contentManager,
                contentTypePartDefinition,
                nodeWithOutgoingRelationships,
                graphSyncHelper,
                graphValidationHelper,
                expectedRelationshipCounts);
        }
    }
}
