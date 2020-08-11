using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Items
{
    public class ContentItemGraphSyncer : IContentItemGraphSyncer
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentPartGraphSyncer> _partSyncers;
        private readonly ILogger<ContentItemGraphSyncer> _logger;
        public int Priority => int.MinValue;

        public ContentItemGraphSyncer(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentPartGraphSyncer> partSyncers,
            ILogger<ContentItemGraphSyncer> logger)
        {
            _partSyncers = partSyncers.OrderByDescending(s => s.Priority);
            _contentDefinitionManager = contentDefinitionManager;
            _logger = logger;
        }

        public bool CanSync(ContentItem contentItem) => true;

        public async Task AllowSync(IGraphMergeItemSyncContext context, IAllowSyncResult allowSyncResult)
        {
            await IteratePartSyncers(context,
                async (partSyncer, partContent) => await partSyncer.AllowSync(partContent, context, allowSyncResult));
        }

        //todo: rename IAllowSyncResult
        public async Task AllowDelete(IGraphDeleteItemSyncContext context, IAllowSyncResult allowSyncResult)
        {
            await IteratePartSyncers(context,
                async (partSyncer, partContent) => await partSyncer.AllowDelete(partContent, context, allowSyncResult));
        }

        public async Task AddSyncComponents(IGraphMergeItemSyncContext context)
        {
            await IteratePartSyncers(context,
                async (partSyncer, partContent) => await partSyncer.AddSyncComponents(partContent, context));
        }

        public async Task DeleteComponents(
            IGraphDeleteItemSyncContext context,
            Func<IContentPartGraphSyncer, JObject?, Task> action)
        {
            await IteratePartSyncers(context,
                async (partSyncer, partContent) => await partSyncer.DeleteComponents(partContent, context));
        }

        public async Task IteratePartSyncers(
            IItemSyncContext context,
            Func<IContentPartGraphSyncer, JObject, Task> action)
        {
            foreach (var partSync in _partSyncers)
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

                    await action(partSync, partContent);
                }
            }
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
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

                dynamic? partContent = context.ContentItem.Content[contentTypePartDefinition.Name];
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
                    context.ContentItem, contentTypePartDefinition.PartDefinition.Name, partContent);
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
