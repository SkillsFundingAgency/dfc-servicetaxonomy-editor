using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Neo4j.Generators;
using DFC.ServiceTaxonomy.Editor.Module.Neo4j.Services;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Notify;
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
        public SyncToGraphTask(IStringLocalizer<SyncToGraphTask> localizer, INeoGraphDatabase neoGraphDatabase,
            IContentManager contentManager, IContentDefinitionManager contentDefinitionManager,
            INotifier notifier)
        {
            _neoGraphDatabase = neoGraphDatabase;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _notifier = notifier;
            T = localizer;
            _relationshipTypeRegex = new Regex("\\[:(.*?)\\]", RegexOptions.Compiled);
        }

        private const string NcsPrefix = "ncs__";

        private IStringLocalizer T { get; }
        private readonly INeoGraphDatabase _neoGraphDatabase;
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly INotifier _notifier;
        private readonly Regex _relationshipTypeRegex;

        public override string Name => nameof(SyncToGraphTask);
        public override LocalizedString DisplayText => T["Sync content item to Neo4j graph"];
        public override LocalizedString Category => T["National Careers Service"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
//            return Outcomes(T["Done"], T["Failed"]);
            return Outcomes(T["Done"]);
        }

        //todo: if this fails, we notify the user, but the content still gets added to oc, and oc & the graph are then out-of-sync.
        // we need to think of the best way to handle it. the event appears to trigger after the content is created (check)
        // we don't want to remove the content in orchard core, as we don't want the user to have to reenter content
        // perhaps we could mark the content as not synced (part of the graph content part?), and either the user can retry
        // (and allow the user to filter by un-synced content)
        // or we have a facility to check & sync all content in oc
        //todo: exported content had prefix/graph/guid <- where did graph come from??
        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            try
            {
                var contentItem = (ContentItem) workflowContext.Input["ContentItem"];

                // we use the existence of a Graph content part as a marker to indicate that the content item should be synced
                // so we silently noop if it's not present
                dynamic graph = ((JObject) contentItem.Content)["Graph"];
                if (graph == null)
                    return Outcomes("Done");

                string nodeUri = graph.UriId.Text.ToString();
                var setMap = new Dictionary<string, object>
                {
                    {"skos__prefLabel", contentItem.Content.TitlePart.Title.ToString()},
                    {"uri", nodeUri}
                };

                var relationships = new Dictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>>();

                foreach (dynamic? field in contentItem.Content[contentItem.ContentType])
                {
                    if (field == null)
                        continue;

                    var fieldTypeAndValue = (JProperty?)((JProperty) field).First?.First;
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

                            string? relationshipType = null;
                            ContentTypeDefinition contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
                            IEnumerable<ContentPartFieldDefinition> contentPartDefinitions
                                = contentTypeDefinition.Parts.First(p => p.Name == contentItem.ContentType).PartDefinition.Fields;
                            ContentPartFieldDefinition contentPartDefinition = contentPartDefinitions.First(d => d.Name == field.Name);
                            string? contentPartHint = contentPartDefinition.Settings["ContentPickerFieldSettings"]?["Hint"]?.ToString();
                            if (contentPartHint != null)
                            {
                                Match match = _relationshipTypeRegex.Match(contentPartHint);
                                if (match.Success)
                                {
                                    relationshipType = $"{match.Groups[1].Value}";
                                }
                            }

                            string? destNodeLabel = null;
                            var destUris = new List<string>();
                            foreach (JToken relatedContentId in fieldTypeAndValue.Value)
                            {
                                ContentItem relatedContent = await _contentManager.GetAsync(relatedContentId.ToString(), VersionOptions.Latest);
                                string relatedContentKey = relatedContent.Content.UriId.URI.Text.ToString();
                                destUris.Add(relatedContentKey.ToString());

                                //todo: don't repeat
                                destNodeLabel = NcsPrefix + relatedContent.ContentType;
                                if (relationshipType == null)
                                    relationshipType = $"{NcsPrefix}has{relatedContent.ContentType}";
                            }
                            if (destNodeLabel != null && relationshipType != null)
                                relationships.Add((destNodeLabel, "uri", relationshipType), destUris);
                            break;
                    }
                }

                string nodeLabel = NcsPrefix + contentItem.ContentType;

                // could create ienumerable and have 1 call
                Statement mergeNodesStatement = StatementGenerator.MergeNodes(nodeLabel, setMap);
                if (relationships.Any())
                {
                    await _neoGraphDatabase.RunWriteStatements(mergeNodesStatement,
                        StatementGenerator.MergeRelationships(nodeLabel, "uri", nodeUri, relationships));
                }
                else
                {
                    await _neoGraphDatabase.RunWriteStatements(mergeNodesStatement);
                }

                return Outcomes("Done");

                //todo: create a uri on on create, read-only when editing (and on create prepopulated?)
            }
            catch (Exception ex)
            {
                // setting this, but not letting the exception propagate doesn't work
                //workflowContext.Fault(ex, activityContext);

//                _notifier.Add(new GetProperty<NotifyType>(), new LocalizedHtmlString(nameof(SyncToGraphTask), $"Sync to graph failed: {ex.Message}"));
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(SyncToGraphTask), $"Sync to graph failed: {ex.Message}"));


                // if we do this, we can trigger a notify task in the workflow from a failed outcome, but the workflow doesn't fault
                //return Outcomes("Failed");
                throw;
            }
        }
    }
}
