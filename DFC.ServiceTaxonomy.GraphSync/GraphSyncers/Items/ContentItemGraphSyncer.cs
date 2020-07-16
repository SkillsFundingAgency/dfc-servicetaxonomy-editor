using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Managers.Interface;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Items
{
    public class ContentItemGraphSyncer : IContentItemGraphSyncer
    {
        private readonly ICustomContentDefintionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentPartGraphSyncer> _partSyncers;
        private readonly ILogger<ContentItemGraphSyncer> _logger;
        public int Priority => int.MinValue;

        public bool CanSync(ContentItem contentItem) => true;

        public ContentItemGraphSyncer(
            ICustomContentDefintionManager contentDefinitionManager,
            IEnumerable<IContentPartGraphSyncer> partSyncers,
            ILogger<ContentItemGraphSyncer> logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _partSyncers = partSyncers;
            _logger = logger;
        }

        public async Task AddSyncComponents(IGraphMergeItemSyncContext context)
        {
            //todo: use Priority instead?
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

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            ContentItem contentItem,
            IValidateAndRepairItemSyncContext context)
        {
            foreach (ContentTypePartDefinition contentTypePartDefinition in context.ContentTypeDefinition.Parts)
            {
                IContentPartGraphSyncer partSyncer = _partSyncers.SingleOrDefault(ps =>
                    ps.CanSync(contentTypePartDefinition.ContentTypeDefinition.Name,
                        contentTypePartDefinition.PartDefinition));

                if (partSyncer == null)
                {
                    _logger.LogInformation(
                        $"No IContentPartGraphSyncer registered to sync/validate {contentTypePartDefinition.ContentTypeDefinition.Name} parts, so ignoring");
                    continue;
                }

                dynamic? partContent = contentItem.Content[contentTypePartDefinition.Name];
                if (partContent == null)
                    continue; //todo: throw??

                // pass as param, rather than context?
                context.ContentTypePartDefinition = contentTypePartDefinition;

                (bool validated, string partFailureReason) = await partSyncer.ValidateSyncComponent(
                    (JObject)partContent, context);

                if (validated)
                    continue;

                string failureReason = $"{partSyncer.PartName} did not validate: {partFailureReason}";
                string failureContext = FailureContext(failureReason, context, contentTypePartDefinition,
                    contentItem, contentTypePartDefinition.PartDefinition.Name, partContent);
                return (false, failureContext);
            }

            return (true, "");
        }

        //todo: output relationships destination label user id, instead of node id
        private string FailureContext(
            string failureReason,
            IValidateAndRepairItemSyncContext context,
            ContentTypePartDefinition contentTypePartDefinition,
            ContentItem contentItem,
            string partName,
            dynamic partContent)
        {
            return $@"{context.ValidateAndRepairGraph.FailureContext(failureReason, contentItem)}
part type name: '{partName}'
     part name: '{contentTypePartDefinition.Name}'
  part content:
{partContent}
Source Node ------------------------------------
        ID: {context.NodeWithOutgoingRelationships.SourceNode.Id}
   user ID: {context.NodeId}
    labels: ':{string.Join(":", context.NodeWithOutgoingRelationships.SourceNode.Labels)}'
properties:
{string.Join(Environment.NewLine, context.NodeWithOutgoingRelationships.SourceNode.Properties.Select(p => $"{p.Key} = {(p.Value is IEnumerable<object> values ? string.Join(",", values.Select(v => v.ToString())) : p.Value)}"))}
Relationships ----------------------------------
{string.Join(Environment.NewLine, context.NodeWithOutgoingRelationships.OutgoingRelationships.Select(or => $"[:{or.Relationship.Type}]->({or.DestinationNode.Id})"))}";
        }
    }
}
