using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Services;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

//todo: content delete

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
            var contentItem = (ContentItem) workflowContext.Input["ContentItem"];

            // custom contentpart that prepopulates, readonly on create {ncsnamespaceconst}{contentItem.ContentType}{generated guid}
            // else, on create content generate the uri here

            var nodeUri = contentItem.Content.UriId.URI.Text.ToString();
            var setMap = new Dictionary<string, object>
            {
                {"skos__prefLabel", contentItem.Content.TitlePart.Title.ToString()},
                {"uri", nodeUri}
            };

            var relationships = new Dictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>>();
            
            foreach (var field in contentItem.Content[contentItem.ContentType])
            {
                var fieldTypeAndValue = (JProperty)((JProperty) field).First.First;
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
                        setMap.Add(NcsPrefix+field.Name, fieldTypeAndValue.Value.ToString());
                        break;
                    case "Value":
                        // orchard always converts entered value to real 2.0 (float/double/decimal)
                        // todo: how to decide whether to convert to driver/cypher's long/integer or float/float? metadata field to override default of int to real? 

                        setMap.Add(NcsPrefix+field.Name, (long)fieldTypeAndValue.Value.ToObject(typeof(long)));
                        break;
                    case "ContentItemIds":
                        //todo: check for empty list => noop, except for initial delete
                        //todo: relationship type from metadata?
                        string destNodeLabel = string.Empty, relationshipType = string.Empty;
                        var destUris = new List<string>();
                        foreach (var relatedContentId in fieldTypeAndValue.Value)
                        {
                            var relatedContent = await _contentManager.GetAsync(relatedContentId.ToString(), VersionOptions.Latest);
                            var relatedContentKey = relatedContent.Content.UriId.URI.Text.ToString();
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
            //todo: transaction is keep as 2 calls
            await _neoGraphDatabase.MergeNode(nodeLabel, setMap);
            await _neoGraphDatabase.MergeRelationships(nodeLabel, "uri", nodeUri, relationships);
            
            return Outcomes("Done");
            
            //todo: create a uri on on create, read-only when editing (and on create prepopulated?)
        }
    }
}