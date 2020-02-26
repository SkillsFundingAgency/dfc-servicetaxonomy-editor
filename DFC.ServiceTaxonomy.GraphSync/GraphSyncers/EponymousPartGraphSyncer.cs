using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
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
    /// Numeric            Value        long            Integer                       xsd:integer       \ OC UI has only numeric, which it presents as a real in content. currently we map to decimal. we could check the field's scale and map appropriately
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

        public async Task<IEnumerable<Query>> AddSyncComponents(
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
                JProperty? firstProperty = (JProperty?) fieldContent?.FirstOrDefault();
                if (firstProperty == null)
                    continue;

                JProperty? secondProperty = (JProperty?) fieldContent.Skip(1).FirstOrDefault();
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
                            AddNumericProperties(mergeNodeCommand, field.Name, firstProperty.Value);
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

                //alternatively, have dic of key to look for and commandiaye

                // int _ = (fieldTypeAndValue.Name, linkTextTypeAndValue?.Name) switch
                // {
                //     ("Text", null) => throw new Exception(),
                //     (_, _) => throw new Exception()
                // };

                //alternatively, have dic of key to look for and commandiaye
                // switch (fieldTypeAndValue.Name)
                // {
                //     // we map from Orchard Core's types to Neo4j's driver types (which map to cypher type)
                //     // see remarks to view mapping table
                //     // we might also want to map to rdf types here (accept flag to say store with type?)
                //     // will be useful if we import into neo using keepCustomDataTypes
                //     // we can append the datatype to the value, i.e. value^^datatype
                //     // see https://neo4j-labs.github.io/neosemantics/#_handling_custom_data_types
                //
                //     case "Text":
                //     case "Html":
                //         mergeNodeCommand.Properties.Add(NcsPrefix + field.Name, fieldTypeAndValue.Value.ToString());
                //         break;
                //     case "Value":
                //         // orchard always converts entered value to real 2.0 (float)
                //         // todo: how to decide whether to convert to driver/cypher's long/integer or float/float? metadata field to override default of int to real?
                //
                //         if (fieldTypeAndValue.Value.Type == JTokenType.Float)
                //             mergeNodeCommand.Properties.Add(NcsPrefix + field.Name, (decimal?) fieldTypeAndValue.Value.ToObject(typeof(decimal)));
                //         break;
                //     case "Url":    // can we rely on Url always being the first property?
                //         const string linkUrlPostfix = "_url";
                //         const string linkTextPostfix = "_text";
                //         mergeNodeCommand.Properties.Add($"{NcsPrefix}{field.Name}{linkUrlPostfix}", fieldTypeAndValue.Value.ToString());
                //         JProperty? linkTextTypeAndValue = (JProperty?) renameMe.Skip(1).FirstOrDefault();
                //         mergeNodeCommand.Properties.Add($"{NcsPrefix}{field.Name}{linkTextPostfix}", linkTextTypeAndValue!.Value.ToString());
                //         break;
                //     case "ContentItemIds":
                //         await AddContentPickerFieldSyncComponents(replaceRelationshipsCommand, fieldTypeAndValue, contentTypePartDefinition, ((JProperty)field).Name);
                //         break;
                // }
            }
            return Enumerable.Empty<Query>();
        }

        private static void AddTextOrHtmlProperties(IMergeNodeCommand mergeNodeCommand, string fieldName, JToken propertyValue)
        {
            mergeNodeCommand.Properties.Add(NcsPrefix + fieldName, propertyValue.ToString());

        }

        private static void AddNumericProperties(IMergeNodeCommand mergeNodeCommand, string fieldName, JToken propertyValue)
        {
            // orchard always converts entered value to real 2.0 (float)
            // todo: convert to driver/cypher's long/integer or float/float depending on scale set on numeric field?

            if (propertyValue.Type == JTokenType.Float)
            {
                decimal? value = (decimal?)propertyValue.ToObject(typeof(decimal));
                if (value == null)    //todo: ok??
                    return;

                mergeNodeCommand.Properties.Add(NcsPrefix + fieldName, value);
            }
        }

        private static void AddLinkProperties(IMergeNodeCommand mergeNodeCommand, string fieldName, string url, string text)
        {
            const string linkUrlPostfix = "_url", linkTextPostfix = "_text";

            mergeNodeCommand.Properties.Add($"{NcsPrefix}{fieldName}{linkUrlPostfix}", url);
            mergeNodeCommand.Properties.Add($"{NcsPrefix}{fieldName}{linkTextPostfix}", text);
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
                new[] {destNodeLabel},
                _graphSyncPartIdProperty.Name,
                destIds);
        }

        private object GetSyncId(ContentItem pickedContentItem)
        {
            return _graphSyncPartIdProperty.Value(pickedContentItem.Content[nameof(GraphSyncPart)]);
        }
    }
}
