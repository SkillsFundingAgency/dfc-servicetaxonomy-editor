using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Managers.Interface;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Items
{
    public class ContentItemGraphSyncer : IContentItemGraphSyncer
    {
        private readonly ICustomContentDefintionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentPartGraphSyncer> _partSyncers;
        public int Priority => int.MinValue;

        public bool CanSync(ContentItem contentItem) => true;

        public ContentItemGraphSyncer(
            ICustomContentDefintionManager contentDefinitionManager,
            IEnumerable<IContentPartGraphSyncer> partSyncers)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _partSyncers = partSyncers;
        }

        public async Task AddSyncComponents(IGraphMergeItemSyncContext context)
        {
            //todo: use Priority isntead?
            // ensure graph sync part is processed first, as other part syncers (current bagpart) require the node's id value
            string graphSyncPartName = nameof(GraphSyncPart);

            //order in ctor?
            // add priority field and order?
            var partSyncersWithGraphLookupFirst
                = _partSyncers.Where(ps => ps.PartName != graphSyncPartName)
                    .Prepend(_partSyncers.First(ps => ps.PartName == graphSyncPartName));

            foreach (var partSync in partSyncersWithGraphLookupFirst)
            {
                // bag part has p.Name == <<name>>, p.PartDefinition.Name == "BagPart"
                // (other non-named parts have the part name in both)

                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
                var contentTypePartDefinitions =
                    contentTypeDefinition.Parts.Where(p => partSync.CanSync(context.ContentItem.ContentType, p.PartDefinition));

                foreach (var contentTypePartDefinition in contentTypePartDefinitions)
                {
                    context.ContentTypePartDefinition = contentTypePartDefinition;

                    string namedPartName = contentTypePartDefinition.Name;

                    JObject? partContent = context.ContentItem.Content[namedPartName];
                    if (partContent == null)
                        continue; //todo: throw??

                    await partSync.AddSyncComponents(partContent, context);
                }
            }
        }

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(ContentItem contentItem, IValidateAndRepairContext context) => throw new System.NotImplementedException();
    }
}
