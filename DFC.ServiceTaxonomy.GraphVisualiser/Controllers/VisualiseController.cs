﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphLookup.Models;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.GraphVisualiser.Services;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Fields;
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

namespace DFC.ServiceTaxonomy.GraphVisualiser.Controllers
{
    public class VisualiseController : Controller
    {
        private readonly IGraphCluster _neoGraphCluster;
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly INeo4JToOwlGeneratorService _neo4JToOwlGeneratorService;
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly IOrchardToOwlGeneratorService _orchardToOwlGeneratorService;
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly IEnumerable<IContentPartGraphSyncer> _partSyncers;
        private readonly IEnumerable<IContentFieldGraphSyncer> _fieldSyncers;
        private readonly IServiceProvider _serviceProvider;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private IContentItemVersion? contentItemVersion = null;

        public VisualiseController(
            IGraphCluster neoGraphCluster,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            INeo4JToOwlGeneratorService neo4jToOwlGeneratorService,
            IGraphSyncHelper graphSyncHelper,
            IOrchardToOwlGeneratorService orchardToOwlGeneratorService,
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion,
            IEnumerable<IContentPartGraphSyncer> contentPartGraphSyncers,
            IEnumerable<IContentFieldGraphSyncer> contentFieldsGraphSyncers,
            IServiceProvider serviceProvider)
        {
            _neoGraphCluster = neoGraphCluster ?? throw new ArgumentNullException(nameof(neoGraphCluster));
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager ?? throw new ArgumentNullException(nameof(contentDefinitionManager));
            _neo4JToOwlGeneratorService = neo4jToOwlGeneratorService ?? throw new ArgumentNullException(nameof(neo4jToOwlGeneratorService));
            _graphSyncHelper = graphSyncHelper;
            _orchardToOwlGeneratorService = orchardToOwlGeneratorService ?? throw new ArgumentNullException(nameof(orchardToOwlGeneratorService));
            _publishedContentItemVersion = publishedContentItemVersion;
            _previewContentItemVersion = previewContentItemVersion;
            _partSyncers = contentPartGraphSyncers;
            _serviceProvider = serviceProvider;
            _fieldSyncers = contentFieldsGraphSyncers;
        }

        public ActionResult Viewer()
        {
            return View();
        }

        public async Task<ActionResult> Data([FromQuery] string? contentItemId, [FromQuery] string? graph)
        {
            ValidateParameters(graph);

            if (string.IsNullOrWhiteSpace(contentItemId))
            {
                return GetOntology();
            }

            if (graph!.ToLowerInvariant() == "published")
            {
                contentItemVersion = _publishedContentItemVersion;
            }
            else
            {
                contentItemVersion = _previewContentItemVersion;
            }

            return await GetData(contentItemId, graph);
        }

        private static void ValidateParameters(string? graph)
        {
            if (string.IsNullOrEmpty(graph))
            {
                throw new ArgumentNullException(nameof(graph));
            }
        }

        private ActionResult GetOntology()
        {
            var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions();

            var owlDataModel = _orchardToOwlGeneratorService.CreateOwlDataModels(contentTypeDefinitions);
            var owlResponseString = JsonSerializer.Serialize(owlDataModel, _jsonOptions);

            return Content(owlResponseString, MediaTypeNames.Application.Json);
        }

        private static string[] RelationshipParts = { nameof(GraphLookupPart) };
        private static string[] RelationshipFields = { nameof(ContentPickerField) };

        private async Task GetRelationshipsForContentItem(ContentItem contentItem, IDescribeRelationshipsContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            //var fields =
            //    contentTypeDefinition.Parts.SelectMany(p => p.PartDefinition.Fields);

            //var relationshipFields =
            //    fields.Where(f => RelationshipFields.Contains(f.FieldDefinition.Name));

            _graphSyncHelper.ContentType = contentItem.ContentType;

            //dynamic? graphSyncPartContent = contentItem.Content[nameof(GraphSyncPart)];
            //List<object> destIdPropertyValues = new List<object>();
            var itemContext = new DescribeRelationshipsContext(contentItem, _graphSyncHelper, _contentManager, _publishedContentItemVersion, context, _serviceProvider);
            foreach (var part in contentTypeDefinition.Parts)
            {
                var partSyncer = _partSyncers.FirstOrDefault(x => x.PartName == part.Name);

                if (partSyncer != null)
                {
                    await partSyncer.AddRelationship(itemContext);
                }

                foreach (var relationshipField in part.PartDefinition.Fields)
                {
                    //var fieldContext = new DescribeRelationshipsContext(contentItem, _graphSyncHelper, _contentManager, _publishedContentItemVersion, partContext, _serviceProvider);
                    itemContext.SetContentPartFieldDefinition(relationshipField);

                    var fieldSyncer = _fieldSyncers.FirstOrDefault(x => x.FieldTypeName == relationshipField.FieldDefinition.Name);

                    var contentItemIds =
                           (JArray)contentItem.Content[relationshipField.PartDefinition.Name][relationshipField.Name]
                               .ContentItemIds;

                    if (contentItemIds != null)
                    {
                        foreach (var relatedContentItemId in contentItemIds)
                        {
                            await GetRelationshipsForContentItem(await _contentManager.GetAsync(relatedContentItemId.ToString()), itemContext);
                        }
                    }

                    if (fieldSyncer != null)
                    {
                        await fieldSyncer.AddRelationship(itemContext);
                    }

                }

                context.AddChildContext(itemContext);
            }
        }

        private async Task<ActionResult> GetData(string contentItemId, string graph)
        {
            //todo: don't need contenttype!
            ContentItem contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Published);
            var rootContext = new DescribeRelationshipsContext(contentItem, _graphSyncHelper, _contentManager, _publishedContentItemVersion, null, _serviceProvider);
            await GetRelationshipsForContentItem(contentItem, rootContext);
            Console.WriteLine(graph);


            //var nodeIdToRelationships = new Dictionary<string, OutgoingRelationships>();

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

            //var relationshipParts =
            //    contentTypeDefinition.Parts.Where(p => RelationshipParts.Contains(p.PartDefinition.Name));

            // var relationshipFields =
            //     contentTypeDefinition.Parts.SelectMany(p => p.PartDefinition.Fields)
            //         .Where(f => RelationshipFields.Contains(f.FieldDefinition.Name));

            //alternatively work though the parts

            //todo: keep recursing (perhaps in an iterator fashion not to blow stack) following relationship fields
            //create query to get everything, either in 1 go, or 1 query per level
            // match (s)-[r]-(d) where d.uri in map
            // match (s)-[r]-(d) where d.userid in map




            //foreach (var relationshipField in relationshipFields)
            //{
            //    //todo: inject IEnumerable field handlers
            //    switch (relationshipField.FieldDefinition.Name)
            //    {
            //        case nameof(ContentPickerField):
            //            //todo: have array of objects {fieldname, contentfieldname}
            //            var contentItemIds =
            //                (JArray)contentItem.Content[relationshipField.PartDefinition.Name][relationshipField.Name]
            //                    .ContentItemIds;

            //            //ContentPickerFieldSettings contentPickerFieldSettings =
            //            //    relationshipField.GetSettings<ContentPickerFieldSettings>();

            //            //string relationshipType =
            //            //    await contentPickerFieldSettings.RelationshipType(_graphSyncHelper);

            //            if (contentItemIds.Any())
            //            {
            //                ContentItem? relatedContentItem = null;

            //                foreach (var relatedContentItemId in contentItemIds)
            //                {
            //                    // use whenall, now getasync supports it
            //                    relatedContentItem = await _contentManager.GetAsync(relatedContentItemId.ToString(),
            //                        VersionOptions.Published);
            //                    //todo: check it's got one!
            //                    var relatedGraphSyncPart = relatedContentItem.Content.GraphSyncPart;

            //                    //todo: need to get idpropertyname and value
            //                    //todo: need GraphSyncHelper for this bit
            //                    //var nodeid = relatedGraphSyncPart.Text.ToString();

            //                    destIdPropertyValues.Add(_graphSyncHelper.GetIdPropertyValue(relatedGraphSyncPart, contentItemVersion));
            //                }
            //            }
            //            break;
            //    }
            //}

            //var nodeWithOutgoingRelationships = new NodeAndOutRelationshipsAndTheirInRelationshipsQuery(await _graphSyncHelper.NodeLabels(), _graphSyncHelper.IdPropertyName(), _graphSyncHelper.GetIdPropertyValue(graphSyncPartContent, contentItemVersion), destIdPropertyValues.Select(x => (string)x).ToList());

            //var result = await _neoGraphCluster.Run(graph, nodeWithOutgoingRelationships);

            //var relationshipResult = result.FirstOrDefault();
            //var owlDataModel = _neo4JToOwlGeneratorService.CreateOwlDataModels(relationshipResult!.SourceNode.Id, relationshipResult.OutgoingRelationships.Select(x => x.outgoingRelationship.DestinationNode).Union(new List<INode>() { relationshipResult.SourceNode }), relationshipResult.OutgoingRelationships.Select(z => z.outgoingRelationship.Relationship).ToHashSet<IRelationship>(), "skos__prefLabel");
            //var owlResponseString = JsonSerializer.Serialize(owlDataModel, _jsonOptions);
            var owlResponseString = "{}";

            return Content(owlResponseString, MediaTypeNames.Application.Json);
        }
    }

    //todo: more generic name
    public class VisualizerQuery
    {
    }
}
