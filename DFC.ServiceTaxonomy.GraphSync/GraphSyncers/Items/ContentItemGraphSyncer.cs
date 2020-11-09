using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Orchestrators;
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

        public async Task AllowSync(IGraphMergeItemSyncContext context, IAllowSync allowSync)
        {
            await IteratePartSyncers(context,
                async (partSyncer, partContent) => await partSyncer.AllowSync(partContent, context, allowSync),
                async (partSyncer, partContent) => await partSyncer.AllowSyncDetaching(context, allowSync));
        }

        public async Task AllowDelete(IGraphDeleteItemSyncContext context, IAllowSync allowSync)
        {
            await IteratePartSyncers(context,
                async (partSyncer, partContent) => await partSyncer.AllowDelete(partContent, context, allowSync));
        }

        public async Task AddSyncComponents(IGraphMergeItemSyncContext context)
        {
            await IteratePartSyncers(context,
                async (partSyncer, partContent) => await partSyncer.AddSyncComponents(partContent, context),
                async (partSyncer, partContent) => await partSyncer.AddSyncComponentsDetaching(context));
        }

        public async Task DeleteComponents(IGraphDeleteItemSyncContext context)
        {
            await IteratePartSyncers(context,
                async (partSyncer, partContent) => await partSyncer.DeleteComponents(partContent, context));
        }

        public async Task MutateOnClone(ICloneItemSyncContext context)
        {
            await IteratePartSyncers(context,
                async (partSyncer, partContent) => await partSyncer.MutateOnClone(partContent, context));
        }

        private async Task IteratePartSyncers(
            IItemSyncContext context,
            Func<IContentPartGraphSyncer, JObject, Task> action,
            Func<IContentPartGraphSyncer, JObject, Task>? detachingPartAction = null)
        {
            foreach (var partSync in _partSyncers)
            {
                // bag part has p.Name == <<name>>, p.PartDefinition.Name == "BagPart"
                // (other non-named parts have the part name in both)
                //todo: contentTypeDefinition can be moved outside of foreach
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

                    if (detachingPartAction != null && context.ContentTypePartDefinition.PartDefinition
                        .Settings["ContentPartSettings"]?[ContentTypeOrchestrator.ZombieFlag]?.Value<bool>() == true)
                    {
                        // we're syncing after this part has been detached from the item
                        await detachingPartAction(partSync, partContent);
                    }
                    else
                    {
                        await action(partSync, partContent);
                    }
                }
            }
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            IValidateAndRepairItemSyncContext context)
        {
            foreach (ContentTypePartDefinition contentTypePartDefinition in context.ContentTypeDefinition.Parts)
            {
                IContentPartGraphSyncer? partSyncer = _partSyncers.SingleOrDefault(ps =>
                    ps.CanSync(contentTypePartDefinition.ContentTypeDefinition.Name,
                        contentTypePartDefinition.PartDefinition));

                if (partSyncer == null)
                {
                    _logger.LogInformation("No IContentPartGraphSyncer registered to sync/validate {ContentTypeDefinitionName} parts, so ignoring.",
                        contentTypePartDefinition.ContentTypeDefinition.Name);
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

        public async Task AddRelationship(IDescribeRelationshipsContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);

            //todo: this is missing a CanSync check, can we replace it with IteratePartSyncers?
            foreach (var partSync in _partSyncers)
            {
                foreach (var contentTypePartDefinition in contentTypeDefinition.Parts)
                {
                    string namedPartName = contentTypePartDefinition.Name;

                    JObject? partContent = context.ContentItem.Content[namedPartName];
                    if (partContent == null)
                        continue;

                    context.ContentTypePartDefinition = contentTypePartDefinition;

                    context.SetContentField(partContent);
                    await partSync.AddRelationship(context);
                }
            }
        }
    }
}
