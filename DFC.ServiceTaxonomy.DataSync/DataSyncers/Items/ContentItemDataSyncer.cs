using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.Helpers;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Orchestrators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Items
{
    public class ContentItemDataSyncer : IContentItemDataSyncer
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentPartDataSyncer> _partSyncers;
        private readonly ILogger<ContentItemDataSyncer> _logger;
        public int Priority => int.MinValue;

        public ContentItemDataSyncer(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentPartDataSyncer> partSyncers,
            ILogger<ContentItemDataSyncer> logger)
        {
            _partSyncers = partSyncers.OrderByDescending(s => s.Priority);
            _contentDefinitionManager = contentDefinitionManager;
            _logger = logger;
        }

        public bool CanSync(ContentItem contentItem) => true;

        public async Task AllowSync(IDataMergeItemSyncContext context, IAllowSync allowSync)
        {
            await IteratePartSyncers(context,
                async (partSyncer, partContent) => await partSyncer.AllowSync(partContent, context, allowSync),
                async (partSyncer, partContent) => await partSyncer.AllowSyncDetaching(context, allowSync));
        }

        public async Task AllowDelete(IDataDeleteItemSyncContext context, IAllowSync allowSync)
        {
            await IteratePartSyncers(context,
                async (partSyncer, partContent) => await partSyncer.AllowDelete(partContent, context, allowSync));
        }

        public async Task AddSyncComponents(IDataMergeItemSyncContext context)
        {
            await IteratePartSyncers(context,
                async (partSyncer, partContent) => await partSyncer.AddSyncComponents(partContent, context),
                async (partSyncer, partContent) => await partSyncer.AddSyncComponentsDetaching(context));
        }

        public async Task DeleteComponents(IDataDeleteItemSyncContext context)
        {
            await IteratePartSyncers(context,
                async (partSyncer, partContent) => await partSyncer.DeleteComponents(partContent, context));
        }

        public async Task MutateOnClone(ICloneItemSyncContext context)
        {
            await IteratePartSyncers(context,
                async (partSyncer, partContent) => await partSyncer.MutateOnClone(partContent, context));
        }

        public async Task AddRelationship(IDescribeRelationshipsItemSyncContext context)
        {
            await IteratePartSyncers(context,
                async (partSyncer, partContent) => await partSyncer.AddRelationship(partContent, context));
        }

        private async Task IteratePartSyncers(
            IItemSyncContext context,
            Func<IContentPartDataSyncer, JObject, Task> action,
            Func<IContentPartDataSyncer, JObject, Task>? detachingPartAction = null)
        {
            foreach (var partSync in _partSyncers)
            {
                // bag part has p.Name == <<name>>, p.PartDefinition.Name == "BagPart"
                // (other non-named parts have the part name in both)
                //todo: contentTypeDefinition can be moved outside of foreach
                var contentTypeDefinition = ContentDefinitionHelper.GetTypeDefinitionCaseInsensitive(
                    context.ContentItem.ContentType,
                    _contentDefinitionManager)!;
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
                IContentPartDataSyncer? partSyncer = _partSyncers.SingleOrDefault(ps =>
                    ps.CanSync(contentTypePartDefinition.ContentTypeDefinition.Name,
                        contentTypePartDefinition.PartDefinition));

                if (partSyncer == null)
                {
                    _logger.LogInformation("No IContentPartDataSyncer registered to sync/validate {ContentTypeDefinitionName} parts, so ignoring.",
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

            return (true, string.Empty);
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
            return
                $@"{context.ValidateAndRepairData.FailureContext(failureReason, contentItem)}
                Part type name: '{partName}'
                Part name: '{contentTypePartDefinition.Name}'
                Part content:

                {partContent}

                Source node ------------------------------------
                {SourceNodeContext(context.NodeWithRelationships.SourceNode, context.NodeId)}
                Outgoing relationships -------------------------
                {string.Join(Environment.NewLine, context.NodeWithRelationships.OutgoingRelationships.Select(or => $"{or.Type}->{or.Id}"))}
                Incoming relationships -------------------------
                {string.Join(Environment.NewLine, context.NodeWithRelationships.IncomingRelationships.Select(or => $"{or.Type}->{or.Id}"))}";
        }

        private string SourceNodeContext(INode? sourceNode, object? nodeId)
        {
            if (sourceNode == null)
                return "N/A";

            return
                $@"Id: {sourceNode.Id}
                User Id: {nodeId}
                Labels: ':{string.Join(",", sourceNode.Labels)}'
                Properties:
                {string.Join(Environment.NewLine, sourceNode.Properties
                    .Select(p => $"{p.Key} = {(p.Value is IEnumerable<object> values ? string.Join(",", values.Select(v => v.ToString())) : p.Value)}"))}";
        }
    }
}
