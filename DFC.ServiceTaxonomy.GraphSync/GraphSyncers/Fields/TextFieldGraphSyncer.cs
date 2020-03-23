﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class TextFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldName => "TextField";

        public async Task AddSyncComponents(
            JObject contentItemField,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentPartFieldDefinition contentPartFieldDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            //todo: helper for this?
            JValue? value = (JValue?)contentItemField["Text"];
            if (value == null || !value.HasValues)
                return;

            mergeNodeCommand.Properties.Add(await graphSyncHelper!.PropertyName(contentPartFieldDefinition.Name), value.ToString(CultureInfo.CurrentCulture));
        }

        public Task<bool> VerifySyncComponent(
            JObject contentItemField,
            //ContentTypePartDefinition contentTypePartDefinition,
            ContentPartFieldDefinition contentPartFieldDefinition,
            INode sourceNode,
            IEnumerable<IRelationship> relationships,
            IEnumerable<INode> destNodes,
            IGraphSyncHelper graphSyncHelper)
        {
            throw new NotImplementedException();
        }
    }
}
