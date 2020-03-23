using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    public class NumericFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldName => "NumericField";

        public async Task AddSyncComponents(
            JObject contentItemField,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentPartFieldDefinition contentPartFieldDefinition,
            IGraphSyncHelper graphSyncHelper)
        {//todo: null value comes as empty string
            JValue? value = (JValue?)contentItemField["Value"];
            if (value == null || !value.HasValues)
                return;

            // // type is null if user hasn't entered a value
            // if (propertyValue.Type != JTokenType.Float)
            //     return;
            //
            // decimal? value = (decimal?)propertyValue.ToObject(typeof(decimal));
            // if (value == null)    //todo: ok??
            //     return;

            //var fieldDefinition = contentTypePartDefinition.PartDefinition.Fields.First(f => f.Name == fieldName);
            var fieldSettings = contentPartFieldDefinition.GetSettings<NumericFieldSettings>();

            string propertyName = await graphSyncHelper!.PropertyName(contentPartFieldDefinition.Name);
            if (fieldSettings.Scale == 0)
            {
                mergeNodeCommand.Properties.Add(propertyName, (int)value);
            }
            else
            {
                mergeNodeCommand.Properties.Add(propertyName, (decimal)value);
            }
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
