using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    /// <summary>Summary to make the build work</summary>
    /// <remarks>
    /// we map from Orchard Core's types to Neo4j's driver types (which map to cypher type)
    /// we might also want to map to rdf types here (accept flag to say store with type?)
    /// will be useful if we import into neo using keepCustomDataTypes
    /// we can append the datatype to the value, i.e. value^^datatype
    /// see https://neo4j-labs.github.io/neosemantics/#_handling_custom_data_types
    ///
    /// Type mappings
    /// -------------
    /// OC UI Field Type | OC Content | Neo Driver    | Cypher     | NSMNTX postfix | RDF             | Notes
    /// Boolean            ?            see notes       Boolean                       xsd:boolean       neo docs say driver is boolean. do they mean Boolean or bool?
    /// Content Picker                                                                                  creates relationships
    /// Date
    /// Date Time
    /// Html               Html         string          String
    /// Link               Url+Text     string+string   String+String
    /// Markdown
    /// Media
    /// Numeric            Value        long            Integer                       xsd:integer       \ OC always present numeric as floats. we check the fields scale to decide whether to store an int or a float
    /// Numeric            Value        float           Float                                           / (RDF supports xsd:int & xsd:integer, are they different or synonyms)
    /// Text               Text         string          String                        xsd:string        no need to specify in RDF - default?
    /// Time
    /// Youtube
    ///
    /// see
    /// https://github.com/neo4j/neo4j-dotnet-driver
    /// https://www.w3.org/2011/rdf-wg/wiki/XSD_Datatypes
    /// https://neo4j.com/docs/labs/nsmntx/current/import/
    /// </remarks>
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
        /// null is a special case to indicate a match when the part is the eponymous named content type part
        /// </summary>
        public string? PartName => null;

        public async Task<IEnumerable<ICommand>> AddSyncComponents(
            dynamic content,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            foreach (dynamic? field in content)
            {
                if (field == null)
                    continue;

                JToken? fieldContent = ((JProperty)field).FirstOrDefault();
                JProperty? firstProperty = (JProperty?)fieldContent?.FirstOrDefault();
                if (firstProperty == null)
                    continue;

                JProperty? secondProperty = (JProperty?)fieldContent.Skip(1).FirstOrDefault();
                string? secondName = secondProperty?.Name;

                if (secondName == null)
                {
                    switch (firstProperty.Name)
                    {
                        case "Text":
                        case "Html":
                            AddTextOrHtmlProperties(mergeNodeCommand, field.Name, firstProperty.Value);
                            break;
                        case "Value":
                            AddNumericProperties(mergeNodeCommand, field.Name, firstProperty.Value, contentTypePartDefinition);
                            break;
                        case "ContentItemIds":
                            await AddContentPickerFieldSyncComponents(replaceRelationshipsCommand, field.Name, firstProperty, contentTypePartDefinition);
                            break;
                    }
                }
                else
                {
                    switch (firstProperty.Name)
                    {
                        case "Url" when secondName == "Text":
                            AddLinkProperties(mergeNodeCommand, field.Name, firstProperty.Name, secondProperty!.Value.ToString());
                            break;
                        case "Text" when secondName == "Url":
                            AddLinkProperties(mergeNodeCommand, field.Name, secondProperty!.Value.ToString(), firstProperty.Name);
                            break;
                    }
                }
            }
            return Enumerable.Empty<ICommand>();
        }

        public async Task<bool> VerifySyncComponent(ContentItem contentItem, ContentTypePartDefinition contentTypePartDefinition, INode sourceNode,
            IEnumerable<IRelationship> relationships, IEnumerable<INode> destNodes)
        {
            foreach (var field in contentTypePartDefinition.PartDefinition.Fields)
            {
                JObject value = contentItem.Content[contentItem.ContentType][field.Name];

                if (field.FieldDefinition.Name == "ContentPickerField")
                {
                    var relationshipType = $"ncs__has{field.Settings["ContentPickerFieldSettings"]!["DisplayedContentTypes"]![0]}";
                    var contentItemIds = (JArray)value["ContentItemIds"]!;

                    var contentCount = contentItemIds.Count;
                    //TODO : need to ignore case for hasSocCode vs hasSOCCode - need to make sure these line up!
                    var relationshipCount = relationships.Count(x => string.Equals(x.Type, relationshipType, StringComparison.CurrentCultureIgnoreCase));

                    if (contentCount != relationshipCount)
                    {
                        return false;
                    }

                    foreach (var item in contentItemIds)
                    {
                        var contentItemId = (string)item!;

                        var destContentItem = await _contentManager.GetAsync(contentItemId);

                        var destUri = (string)destContentItem.Content.GraphSyncPart.Text;

                        var destNode = destNodes.SingleOrDefault(n => (string)n.Properties[_graphSyncPartIdProperty.Name] == destUri);

                        if (destNode == null)
                        {
                            return false;
                        }

                        var relationship = relationships.SingleOrDefault(x =>
                            string.Equals(x.Type, relationshipType, StringComparison.CurrentCultureIgnoreCase) && x.EndNodeId == destNode.Id);

                        if (relationship == null)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    var contentItemValue = value?["Text"] ?? value?["Html"] ?? value?["Value"] ?? value?["Url"];
                    sourceNode.Properties.TryGetValue($"ncs__{field.Name}", out var nodePropertyValue);

                    if (Convert.ToString(contentItemValue) != Convert.ToString(nodePropertyValue))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static void AddTextOrHtmlProperties(IMergeNodeCommand mergeNodeCommand, string fieldName, JToken propertyValue)
        {
            mergeNodeCommand.Properties[NcsPrefix + fieldName] = propertyValue.ToString();
        }

        private static void AddNumericProperties(IMergeNodeCommand mergeNodeCommand, string fieldName, JToken propertyValue, ContentTypePartDefinition contentTypePartDefinition)
        {
            // type is null if user hasn't entered a value
            if (propertyValue.Type != JTokenType.Float)
                return;

            decimal? value = (decimal?)propertyValue.ToObject(typeof(decimal));
            if (value == null)    //todo: ok??
                return;

            var fieldDefinition = contentTypePartDefinition.PartDefinition.Fields.First(f => f.Name == fieldName);
            var fieldSettings = fieldDefinition.GetSettings<NumericFieldSettings>();

            string propertyName = $"{NcsPrefix}{fieldName}";
            if (fieldSettings.Scale == 0)
            {
                mergeNodeCommand.Properties[propertyName] = (int)value;
            }
            else
            {
                mergeNodeCommand.Properties[propertyName] = value;
            }
        }

        private static void AddLinkProperties(IMergeNodeCommand mergeNodeCommand, string fieldName, string url, string text)
        {
            const string linkUrlPostfix = "_url", linkTextPostfix = "_text";

            mergeNodeCommand.Properties[$"{NcsPrefix}{fieldName}{linkUrlPostfix}"] = url;
            mergeNodeCommand.Properties[$"{NcsPrefix}{fieldName}{linkTextPostfix}"] = text;
        }

        //todo: interface for fields?
        private async Task AddContentPickerFieldSyncComponents(
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            string fieldName,
            JProperty contentItemIdsProperty,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            var fieldDefinitions = contentTypePartDefinition.PartDefinition.Fields;

            //todo: firstordefault + ? then log and return if null
            ContentPickerFieldSettings contentPickerFieldSettings = fieldDefinitions
                .First(f => f.Name == fieldName).GetSettings<ContentPickerFieldSettings>();

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

            //todo requires 'picked' part has a graph sync part
            // add to docs & handle picked part not having graph sync part or throw exception

            var destIds = await Task.WhenAll(contentItemIdsProperty.Value.Select(async relatedContentId =>
                GetSyncId(await _contentManager.GetAsync(relatedContentId.ToString(), VersionOptions.Latest))));

            replaceRelationshipsCommand.AddRelationshipsTo(
                relationshipType,
                new[] { destNodeLabel },
                _graphSyncPartIdProperty.Name,
                destIds);
        }

        private object GetSyncId(ContentItem pickedContentItem)
        {
            return _graphSyncPartIdProperty.Value(pickedContentItem.Content[nameof(GraphSyncPart)]);
        }
    }
}
