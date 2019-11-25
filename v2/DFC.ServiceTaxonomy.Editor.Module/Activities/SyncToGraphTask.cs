using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using DFC.ServiceTaxonomy.Editor.Module.Services;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace DFC.ServiceTaxonomy.Editor.Module.Activities
{
    // Type mappings
    // -------------
    // OC UI Field Type | OC Content | Neo Driver    | Cypher     | NSMNTX postfix | RDF             | Notes
    // Boolean            ?            see notes       Boolean                       xsd:boolean       neo docs say driver is boolean. do they mean Boolean or bool?
    // Content Picker
    // Date
    // Date Time
    // Html               Html         string          String
    // Link
    // Markdown
    // Media
    // Numeric            Value        long            Integer                       xsd:integer       \ OC UI has only numeric, which it presents as a real in content. do we always consistently map to a long or float, or allow user to differentiate with metadata?
    // Numeric            Value        float           Float                                           / (RDF supports xsd:int & xsd:integer, are they different or synonyms)
    // Text               Text         string          String                        xsd:string        no need to specify in RDF - default?
    // Time
    // Youtube
    //
    // see
    // https://github.com/neo4j/neo4j-dotnet-driver
    // https://www.w3.org/2011/rdf-wg/wiki/XSD_Datatypes
    // https://neo4j.com/docs/labs/nsmntx/current/import/
    public class SyncToGraphTask : TaskActivity
    {
        public SyncToGraphTask(IStringLocalizer<SyncToGraphTask> localizer, INeoGraphDatabase neoGraphDatabase, IContentManager contentManager)
        {
            _neoGraphDatabase = neoGraphDatabase;
            _contentManager = contentManager;
            T = localizer;
        }

        private const string NcsPrefix = "ncs__";

        private IStringLocalizer T { get; }
        private readonly INeoGraphDatabase _neoGraphDatabase;
        private readonly IContentManager _contentManager;

        public override string Name => nameof(SyncToGraphTask);
        public override LocalizedString DisplayText => T["Sync content item to Neo4j graph"];
//        public override LocalizedString Category => T["Neo4j"];
        public override LocalizedString Category => T["Primitives"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }
//todo: why called twice?
        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            //how to get created/edited? or do we execute cypher that handles both?
//            var description = ((ContentItem)workflowContext.Input["ContentItem"]).Get<>();
//            var description = ((ContentItem)workflowContext.Input["ContentItem"]).Get("Description");
            var contentItem = (ContentItem) workflowContext.Input["ContentItem"];
            //var content = contentItem.Content[contentItem.ContentType];

            //var uri = contentItem.Content.UriId.URI.Text.ToString();
            //var title = contentItem.Content.TitlePart.Title.ToString();

            //var setMap = new JObject();

            // custom contentpart that prepopulates, readonly on create {ncsnamespaceconst}{contentItem.ContentType}{generated guid}
            // else, on create content generate the uri here

            var nodeUri = contentItem.Content.UriId.URI.Text.ToString();
            var setMap = new Dictionary<string, object>
            {
                {"skos__prefLabel", contentItem.Content.TitlePart.Title.ToString()},
                {"uri", nodeUri}
            };

            var relationships =
                new Dictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType),
                    IEnumerable<string>>();
            
            // setMap.Add("skos__prefLabel", contentItem.Content.TitlePart.Title.ToString());
            // // custom contentpart that prepopulates, readonly on create {ncsnamespaceconst}{contentItem.ContentType}{generated guid}
            // // else, on create content generate the uri here
            // setMap.Add("uri", contentItem.Content.UriId.URI.Text.ToString());
            
            var content = (JObject) contentItem.Content;
            foreach (var field in contentItem.Content[contentItem.ContentType])
            {
                var fieldTypeAndValue = (JProperty)((JProperty) field).First.First;
                switch (fieldTypeAndValue.Name)
                {
                    case "Text":
                    case "Html":
                        setMap.Add(NcsPrefix+field.Name, fieldTypeAndValue.Value.ToString());
                        break;
                    case "Value":
                        setMap.Add(NcsPrefix+field.Name, (long)fieldTypeAndValue.Value.ToObject(typeof(long)));
                        break;
                    case "ContentItemIds":
                        //todo: check for empty list => noop, except for initial delete
                        //todo: relationship type from metadata?
                        string destNodeLabel = string.Empty, relationshipType = string.Empty;
                        //var relationshipType = $"{NcsPrefix}has{field.Name}";
                        var destUris = new List<string>();
                        foreach (var relatedContentId in fieldTypeAndValue.Value)
                        { // activity:relatedContent.ContentType
                            var relatedContent = await _contentManager.GetAsync(relatedContentId.ToString(), VersionOptions.Latest);
                            var relatedContentKey = relatedContent.Content.UriId.URI.Text.ToString(); //((JObject)relatedContent.Content).GetValue("uri");
                            destUris.Add(relatedContentKey.ToString());
                            
                            //todo: don't repeat
                            destNodeLabel = NcsPrefix + relatedContent.ContentType;
                            relationshipType = $"{NcsPrefix}has{relatedContent.ContentType}";
                        }
                        relationships.Add((destNodeLabel, "uri", relationshipType), destUris);
                        break;
                }
            }

            var nodeLabel = NcsPrefix + contentItem.ContentType;
            //todo: combine into 1 call? can't concurrent these - nodes need creating first
            await _neoGraphDatabase.MergeNode(nodeLabel, setMap);
            await _neoGraphDatabase.MergeRelationships(nodeLabel, "uri", nodeUri, relationships);
            
            return Outcomes("Done");
            
//             // uri from uri id content part : contentItem.Content.UriId.URI.Text.ToString() | title from titlepart: contentItem.Content.TitlePart.Title.ToString()
//             // if we use title part: ((JObject) contentItem.Content)["TitlePart"]
//             // var titlePart = (JObject) content["TitlePart"];
//             // var title = titlePart.Values().First().ToString();
//             var properties = new Dictionary<string,object>(content[contentItem.ContentType].Cast<JProperty>().Choose(
//                 i =>
//                 {
//                     // move into method
//                     var property = (JProperty) i.First.First;
//                     Type neoType;
//                     // map from Orchard Core's types to Neo4j's driver types (which map to cypher type) perhaps add table in comment to show mapping between entered -> orchard -> driver -> cypher -> rdf
//                     // can we always use a string for the neo type
//                     // we might want to map to rdf types (accept flag to say store with type?)
//                     switch (property.Name)
//                     {
//                         case "Value":        // orchard always convert entered value to real 2.0 (float/double/decimal)
//                             //todo: how to decide whether to convert to driver/cypher's long/integer or float/float? metadata field to override default of int to real? 
//                             neoType = typeof(long);
//                             return (true, new KeyValuePair<string, object>(i.Name, property.Value.ToObject(neoType)));
//                         case "Text":
//                         case "Html":
//                             neoType = typeof(string);
//                             return (true, new KeyValuePair<string, object>(i.Name, property.Value.ToObject(neoType)));
//                         // could just do...
//                             //return (true, new KeyValuePair<string, object>(i.Name, property.Value.ToString()));
//                         case "ContentItemIds": //todo how does content come through when single relationship rather than many
// //                            foreach (var contentId in property.Value)
// //                            {}
// //                            var relationship = _contentManager.GetAsync(property.Value.ToString());
// //                            return (true, new KeyValuePair<string, object>(i.Name, "?"));
// // todo: don't return property, merge relationships immediately
//                             return (false, default);
//                         default:
//                             return (false, default);
//
//                     }
// //                    return new KeyValuePair<string, object>(i.Name, property.Value.ToObject(neoType));
// //                    var q = i.First().Children();
// //                    return new KeyValuePair<string, object>(i.Name, ((JProperty) i.First().Children().First()).Value);
//                 }
//                 ));
//             
//             await _neoGraphDatabase.MergeNode(contentItem.ContentType, properties);

//            ((JObject) contentItem.Content)[contentItem.ContentType].Cast<JProperty>().Select(i =>
//                (Name: i.Name, x: ((JProperty) i.First().Children().First()).Value));
//            var contentY = ((JObject) contentItem.Content)[contentItem.ContentType].Cast<JProperty>()
//                .Select(i => (Name: i.Name, x: i.First()));
//            //i.Value.First.))
//            var contentX = ((JObject) contentItem.Content)[contentItem.ContentType].Cast<JProperty>().Select(i => (i.Name, i.Values()));
            
            // use title part, or have title field?
            // delete
//            var contentJObject = content as JObject;
            
//            foreach (var property in contentJObject)
//            {
//                var key = property.Key;
//                var value = property.Value;
//                var firstChild = value.Children<JProperty>().First();
//                // will be useful if we import into neo using keepCustomDataTypes 
//                // we can append the datatype to the value, i.e. value^^datatype
//                // see https://neo4j-labs.github.io/neosemantics/#_handling_custom_data_types
//                var contentFieldDataType = firstChild.Name;
//                var fieldValue = firstChild.Value;
////                NeoPropertyType neoPropertyType;
////                //or dictionary
////                switch (contentFieldDataType)
////                {
////                    //case "Html":
////                    default:
////                        neoPropertyType = NeoPropertyType.String;
////                        break;
////                }
//                
//                //todo: close/dispose, dictionary properties
//                await _neoGraphDatabase.MergeNode(contentItem.ContentType, key, fieldValue); //, neoPropertyType);
//            }
            
            //todo: create a uri on on create, read-only when editing (and on create prepopulated?)
            
            // return Outcomes("Done");
        }
    }
}