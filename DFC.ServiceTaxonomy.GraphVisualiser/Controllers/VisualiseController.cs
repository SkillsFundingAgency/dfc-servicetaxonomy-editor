using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphLookup.Models;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphVisualiser.Services;
using DFC.ServiceTaxonomy.Neo4j.Services;
using DFC.ServiceTaxonomy.Neo4j.Types;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

// visualise -> https://localhost:44346/Visualise/Viewer?visualise=<the graph sync part url>
// ontology -> https://localhost:44346/Visualise/Viewer?visualise=

//todo:
// add a custom part that selects the visualisation rule/heuristic
// e.g. visualizing a job profile, might want to follow all relationship paths until reach another job profile (follow to self)
// in addition would want to whitelist any esco namespace nodes to occupation and skill (would you want to show skills off occupation when displaying job profile?)
// have it configurable which namespaces to white/black list & specific labels in either list to white black list?
// gonna get quite complicated!
// e.g. for tasks, would probably only want to visualise the task and first degree relationships
// that allows different behaviour for different types, but in a generic manner
// we can set up the existing types sensibly, but any added types the user can set themeselves

// ^^ instead, add property to bagitem created relationships to say bag or embedded or similar
// then follow to 1 degree where relationship doesn't have embedded property, with special handling for esco?

//todo: (a) either group related nodes of same type together (like neo's browser)
// or (b) have combined data/schema visualiser

// (a) atm just overriding colour, so layout engine doesn't know anything about type
// add property to classattribute of type and change renderer
// or check out all existing owl types and see if can piggy-back or build on any

// (b) e.g. (software developer:jobProfile)--(Tasks)--(coding)

//todo: move visualisation into GraphSync module? then could add view graph button to sync part (along with id)
//todo: on properties pop.up have edit button which links to content edit page

//todo: maxLabelWidth to 180 : need to add options support to import json

// settings min degrees, max degrees, max nodes
// 1st pass all relations/bags
// 2+ pass all relations/bags:
//   if node already encountered, add relationship, don't follow further
//   if content type already encountered, don't add node or relationship, don't follow further???
// if 2+ pass takes nodes past max nodes, don't add any of pass (will need to stage)
// ignore maxnodes for 1 degree??

namespace DFC.ServiceTaxonomy.GraphVisualiser.Controllers
{
    public class VisualiseController : Controller
    {
        private readonly IGraphDatabase _neoGraphDatabase;
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly INeo4JToOwlGeneratorService _neo4JToOwlGeneratorService;
        private readonly IOrchardToOwlGeneratorService _orchardToOwlGeneratorService;

        private readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public VisualiseController(
            IGraphDatabase neoGraphDatabase,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IGraphSyncHelper graphSyncHelper,
            INeo4JToOwlGeneratorService neo4jToOwlGeneratorService,
            IOrchardToOwlGeneratorService orchardToOwlGeneratorService)
        {
            _neoGraphDatabase = neoGraphDatabase ?? throw new ArgumentNullException(nameof(neoGraphDatabase));
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager ?? throw new ArgumentNullException(nameof(contentDefinitionManager));
            _graphSyncHelper = graphSyncHelper;
            _neo4JToOwlGeneratorService = neo4jToOwlGeneratorService ?? throw new ArgumentNullException(nameof(neo4jToOwlGeneratorService));
            _orchardToOwlGeneratorService = orchardToOwlGeneratorService ?? throw new ArgumentNullException(nameof(orchardToOwlGeneratorService));
        }

        public ActionResult Viewer()
        {
            return View();
        }

        public async Task<ActionResult> Data([FromQuery] string? contentItemId)
        {
            if (string.IsNullOrWhiteSpace(contentItemId))
            {
                return GetOntology();
            }

            return await GetData(contentItemId);
        }

        private ActionResult GetOntology()
        {
            var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions();

            var owlDataModel = _orchardToOwlGeneratorService.CreateOwlDataModels(contentTypeDefinitions);
            var owlResponseString = JsonSerializer.Serialize(owlDataModel, JsonOptions);

            return Content(owlResponseString, MediaTypeNames.Application.Json);
        }

        //private static string[] GroupingFields = { nameof(TabField), nameof(AccordionField) };
        private static string[] RelationshipParts = { nameof(GraphLookupPart) };
        private static string[] RelationshipFields = { nameof(ContentPickerField) };

        #pragma warning disable S1172
#pragma warning disable S1481
        private async Task<ActionResult> GetData(string contentItemId)
        {
            //todo: don't need contenttype!
            ContentItem contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Published);

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            // const string prefLabel = "skos__prefLabel";
            // var query = new GetNodesCypherQuery(nameof(uri), uri, prefLabel, prefLabel);
            //
            // _ = await _neoGraphDatabase.Run(query);
            //
            // var owlDataModel = Neo4JToOwlGeneratorService.CreateOwlDataModels(query.SelectedNodeId, query.Nodes, query.Relationships, prefLabel);
            // var owlResponseString = JsonSerializer.Serialize(owlDataModel, JsonOptions);

            // var contentTypePartDefinitions =
            //     contentTypeDefinition.Parts.Where(
            //         p => p.PartDefinition.Name == contentItem.ContentType ||
            //              p.PartDefinition.Fields.Any(f => GroupingFields.Contains(f.FieldDefinition.Name)));

            var relationshipParts =
                contentTypeDefinition.Parts.Where(p => RelationshipParts.Contains(p.PartDefinition.Name));

            // var relationshipFields =
            //     contentTypeDefinition.Parts.SelectMany(p => p.PartDefinition.Fields)
            //         .Where(f => RelationshipFields.Contains(f.FieldDefinition.Name));

            //alternatively work though the parts

            //todo: keep recursing (perhaps in an iterator fashion not to blow stack) following relationship fields
            //create query to get everything, either in 1 go, or 1 query per level
            // match (s)-[r]-(d) where d.uri in map
            // match (s)-[r]-(d) where d.userid in map

            var fields =
                contentTypeDefinition.Parts.SelectMany(p => p.PartDefinition.Fields);

            var relationshipFields =
                fields.Where(f => RelationshipFields.Contains(f.FieldDefinition.Name));

            foreach (var relationshipField in relationshipFields)
            {
                //todo: have array of objects {fieldname, contentfieldname}
                var contentItemIds = (JArray)contentItem.Content[relationshipField.PartDefinition.Name][relationshipField.Name].ContentItemIds;

                foreach (var relatedContentItemId in contentItemIds)
                {
                    // use whenall, now getasync supports it
                    var relatedContentItem = await _contentManager.GetAsync(relatedContentItemId.ToString(), VersionOptions.Published);
                    //todo: check it's got one!
                    var relatedGraphSyncPart = relatedContentItem.Content.GraphSyncPart;

                    //todo: need to get idpropertyname and value
                    //todo: need GraphSyncHelper for this bit
                    var nodeid = relatedGraphSyncPart.Text.ToString();

                    //todo: inject IEnumerable field handlers
                    switch (relationshipField.Name)
                    {
                        case "ContentPickerField":
                            ContentPickerFieldSettings contentPickerFieldSettings =
                                relationshipField.GetSettings<ContentPickerFieldSettings>();

                            string relationshipType = await contentPickerFieldSettings.RelationshipType(_graphSyncHelper);
                            break;
                    }


                }
            }

            string owlResponseString = "{}";

            return Content(owlResponseString, MediaTypeNames.Application.Json);
        }
    }

    //todo: IReplaceRelationshipsCommand : IRelationships, ICommand
    //todo: need a name to distinguish between describing relationships for queries/command params and cypher responses
    // irelationship/igraphrelationship??
    public interface IRelationships
    {
        HashSet<string> SourceNodeLabels { get; set; }
        string? SourceIdPropertyName { get; set; }
        object? SourceIdPropertyValue { get; set; }

        IEnumerable<Relationship> Relationships { get; }

        /// <summary>
        /// One relationship will be created for each destIdPropertyValue.
        /// If no destIdPropertyValues are supplied, then no relationships will be created,
        /// but any relationships of relationshipType, from the source node to nodes with destNodeLabels will still be removed.
        /// </summary>
        void AddRelationshipsTo(
            string relationshipType,
            IEnumerable<string> destNodeLabels,
            string destIdPropertyName,
            params object[] destIdPropertyValues);
    }

    //todo: compose ReplaceRelationshipsCommand with this
    public class OutgoingRelationships : IRelationships
    {
        public HashSet<string> SourceNodeLabels { get; set; } = new HashSet<string>();
        public string? SourceIdPropertyName { get; set; }
        public object? SourceIdPropertyValue { get; set; }

        public IEnumerable<Relationship> Relationships
        {
            get { return RelationshipsList; }
        }

        private List<Relationship> RelationshipsList { get; set; } = new List<Relationship>();

        public void AddRelationshipsTo(string relationshipType, IEnumerable<string> destNodeLabels,
            string destIdPropertyName, params object[] destIdPropertyValues)
        {
            RelationshipsList.Add(new Relationship(relationshipType, null, destNodeLabels, destIdPropertyName,
                destIdPropertyValues));
        }

        public List<string> ValidationErrors()
        {
            List<string> validationErrors = new List<string>();

            if (!SourceNodeLabels.Any())
                validationErrors.Add($"Missing {nameof(SourceNodeLabels)}.");

            if (SourceIdPropertyName == null)
                validationErrors.Add($"{nameof(SourceIdPropertyName)} is null.");

            if (SourceIdPropertyValue == null)
                validationErrors.Add($"{nameof(SourceIdPropertyValue)} is null.");

            foreach (var relationship in RelationshipsList)
            {
                var relationshipValidationErrors = relationship.ValidationErrors;
                if (relationshipValidationErrors.Any())
                {
                    validationErrors.Add(
                        $"{relationship.RelationshipType ?? "<Null Type>"} relationship invalid ({string.Join(",", relationshipValidationErrors)})");
                }
            }

            return validationErrors;
        }
    }

}
