﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class DateTimeFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "DateTimeField";

        private const string ContentKey = "Value";

        public async Task AddSyncComponents(JObject contentItemField,
            IContentManager contentManager,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            IContentPartFieldDefinition contentPartFieldDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            JValue? value = (JValue?)contentItemField?[ContentKey];
            if (value == null || value.Type == JTokenType.Null)
                return;

            string propertyName = await graphSyncHelper!.PropertyName(contentPartFieldDefinition.Name);

            mergeNodeCommand.Properties.Add(propertyName, (DateTime)value);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject contentItemField,
            IContentPartFieldDefinition contentPartFieldDefinition,
            IContentManager contentManager,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts)
        {
            string nodePropertyName = await graphSyncHelper.PropertyName(contentPartFieldDefinition.Name);

            return graphValidationHelper.DateTimeContentPropertyMatchesNodeProperty(
               ContentKey,
               contentItemField,
               nodePropertyName,
               nodeWithOutgoingRelationships.SourceNode);
        }
    }
}
