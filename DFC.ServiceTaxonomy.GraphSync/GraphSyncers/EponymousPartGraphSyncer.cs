using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class EponymousPartGraphSyncer : IContentPartGraphSyncer
    {
        private readonly IContentManager _contentManager;
        private readonly IGraphSyncPartIdProperty _graphSyncPartIdProperty;
        private readonly Regex _relationshipTypeRegex;

        //todo: have as setting of activity, or graph sync content part settings
        private const string NcsPrefix = "ncs__";

        public EponymousPartGraphSyncer(IContentManager contentManager,
            IGraphSyncPartIdProperty graphSyncPartIdProperty)
        {
            _contentManager = contentManager;
            _graphSyncPartIdProperty = graphSyncPartIdProperty;
            _relationshipTypeRegex = new Regex("\\[:(.*?)\\]", RegexOptions.Compiled);
        }

        /// <summary>
        /// null is a special case to indicate a match when the part is the special content type part
        /// </summary>
        public string? PartName => null;

        public async Task AddSyncComponents(dynamic content, IDictionary<string, object> nodeProperties,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            foreach (dynamic? field in content)
            {
                if (field == null)
                    continue;

                var fieldTypeAndValue = (JProperty?) ((JProperty) field).First?.First;
                if (fieldTypeAndValue == null)
                    continue;

                switch (fieldTypeAndValue.Name)
                {
                    // we map from Orchard Core's types to Neo4j's driver types (which map to cypher type)
                    // see remarks to view mapping table
                    // we might also want to map to rdf types here (accept flag to say store with type?)
                    // will be useful if we import into neo using keepCustomDataTypes
                    // we can append the datatype to the value, i.e. value^^datatype
                    // see https://neo4j-labs.github.io/neosemantics/#_handling_custom_data_types

                    case "Text":
                    case "Html":
                        nodeProperties.Add(NcsPrefix + field.Name, fieldTypeAndValue.Value.ToString());
                        break;
                    case "Value":
                        // orchard always converts entered value to real 2.0 (float/double/decimal)
                        // todo: how to decide whether to convert to driver/cypher's long/integer or float/float? metadata field to override default of int to real?

                        nodeProperties.Add(NcsPrefix + field.Name, (long) fieldTypeAndValue.Value.ToObject(typeof(long)));
                        break;
                    case "ContentItemIds":
                        // ContentPickerFieldSettings conterPickerField = contentTypePartDefinition.PartDefinition.Fields
                        //     .First(f => f.Name == ((JProperty)field).Name).GetSettings<ContentPickerFieldSettings>();

                        await AddContentPickerFieldSyncComponents(nodeRelationships, /*field,*/ fieldTypeAndValue, contentTypePartDefinition, ((JProperty)field).Name);
                        break;
                }
            }
        }

        //todo: interface for fields?
        private async Task AddContentPickerFieldSyncComponents(
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> relationships,
            //dynamic field,
            JProperty fieldTypeAndValue,
            ContentTypePartDefinition contentTypePartDefinition,
            string contentPickerFieldName)
        {
            //todo: check for empty list => noop, *except for initial delete* -> if none, can't get destnodelabel & relationship type from content. need to get it from settings instead

            var fieldDefinitions = contentTypePartDefinition.PartDefinition.Fields;

            ContentPickerFieldSettings contentPickerFieldSettings = fieldDefinitions.First(f => f.Name == contentPickerFieldName).GetSettings<ContentPickerFieldSettings>();

            string pickedContentType = contentPickerFieldSettings.DisplayedContentTypes[0];

            string? relationshipType = null;
            if (contentPickerFieldSettings.Hint != null)
            {
                Match match = _relationshipTypeRegex.Match(contentPickerFieldSettings.Hint);
                if (match.Success)
                {
                    relationshipType = $"{match.Groups[1].Value}";
                }
            }
            if (relationshipType == null)
                relationshipType = $"{NcsPrefix}has{pickedContentType}";

            string destNodeLabel = NcsPrefix + pickedContentType;


            // JToken firstRelatedContentId = fieldTypeAndValue.Value.FirstOrDefault();
            // if (firstRelatedContentId == default)
            //     return;

            // string? relationshipType = null;
            // ContentPartFieldDefinition contentPartFieldDefinition = fields.First(d => d.Name == field.Name);
            // string? contentPartHint = contentPartFieldDefinition.Settings["ContentPickerFieldSettings"]?["Hint"]?.ToString();
            // if (contentPartHint != null)
            // {
            //     Match match = _relationshipTypeRegex.Match(contentPartHint);
            //     if (match.Success)
            //     {
            //         relationshipType = $"{match.Groups[1].Value}";
            //     }
            // }

            // ContentItem firstRelatedContent = await _contentManager.GetAsync(firstRelatedContentId.ToString(), VersionOptions.Latest);
            //
            // string destNodeLabel = NcsPrefix + firstRelatedContent.ContentType;
            // if (relationshipType == null)
            //     relationshipType = $"{NcsPrefix}has{firstRelatedContent.ContentType}";

            var destUris = new List<string>();
            // {
            //     GetSyncId(firstRelatedContent)
            // };

            foreach (JToken relatedContentId in fieldTypeAndValue.Value) //.Skip(1))
            {
                ContentItem relatedContent = await _contentManager.GetAsync(relatedContentId.ToString(), VersionOptions.Latest);

                //todo requires 'picked' part has a graph sync part
                // add to docs & handle picked part not having graph sync part or throw exception

                destUris.Add(GetSyncId(relatedContent));
            }

            relationships.Add((destNodeLabel, _graphSyncPartIdProperty.Name, relationshipType), destUris);
        }

        private string GetSyncId(ContentItem pickedContentItem)
        {
            return _graphSyncPartIdProperty.Value(pickedContentItem.Content[nameof(GraphSyncPart)]);
        }
    }
}
