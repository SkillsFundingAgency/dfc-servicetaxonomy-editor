﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
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

        public async Task AddRelationship(IDescribeRelationshipsItemSyncContext context)
        {
            await IteratePartSyncers(context,
                async (partSyncer, partContent) => await partSyncer.AddRelationship(partContent, context));
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
                $@"{context.ValidateAndRepairGraph.FailureContext(failureReason, contentItem)}
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
                    .Select(ParseProperties))}";
        }

        private string ParseProperties(KeyValuePair<string, object> property)
        {
            var key = property.Key;
            var propertyValue = property.Value.ToString();
            return $"{key}: {propertyValue}";
        }
    }
}
