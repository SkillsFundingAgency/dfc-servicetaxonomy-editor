﻿using System;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.GraphVisualiser.Services;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement.Metadata;

// visualise -> https://localhost:44346/Visualise/Viewer?visualise=<the graph sync part url>
// ontology -> https://localhost:44346/Visualise/Viewer?visualise=

//todo:
// * syncnameprovider needs to be aware of (and cache) the content types in oc, so that non-oc nodes can be visualised
//   (such as some esco nodes, but also potentially visitor nodes etc.)
// * need to detect and terminate the branch on encountering an already encountered node
//   (although it might still work without doing this)
// * need to set sensible maxdepth values on some (all?) types
// * should we count embedded items (recursively) as part of max depth?
// * move edit item and recenter functionality into display area (pop-outs like neo browser, or right click menu)
// * when get incoming relationships on source node, if incoming node is an embedded item, also show containing item
// that way instead of e.g. shared content showing incoming relationships from HTMLShared widgets
// it shows the pages that use the shared content
// * discriminate nodes with prefixes (so its clear what's actually esco)

//todo: move visualisation into GraphSync module? then could add view graph button to sync part (along with id)
//todo: maxLabelWidth to 180 : need to add options support to import json

namespace DFC.ServiceTaxonomy.GraphVisualiser.Controllers
{
    public class VisualiseController : Controller
    {
        //todo: service(s) for url generation?
        private const string EditBaseUrl = "/Admin/Contents/ContentItems/{ContentItemID}/Edit";
        private const string ResetFocusBaseUrl = "/Visualise/Viewer?contentItemId={ContentItemID}&graph={graph}";
        private const string ContentItemIdPlaceHolder = "{ContentItemID}";
        private const string GraphPlaceHolder = "{graph}";
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly INeo4JToOwlGeneratorService _neo4JToOwlGeneratorService;
        private readonly IOrchardToOwlGeneratorService _orchardToOwlGeneratorService;
        private readonly IVisualiseGraphSyncer _visualiseGraphSyncer;
        private readonly IContentItemVersionFactory _contentItemVersionFactory;
        private readonly INodeContentItemLookup _nodeContentItemLookup;
        private IContentItemVersion? _contentItemVersion;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public VisualiseController(
            IContentDefinitionManager contentDefinitionManager,
            INeo4JToOwlGeneratorService neo4jToOwlGeneratorService,
            IOrchardToOwlGeneratorService orchardToOwlGeneratorService,
            IVisualiseGraphSyncer visualiseGraphSyncer,
            IContentItemVersionFactory contentItemVersionFactory,
            INodeContentItemLookup nodeContentItemLookup)
        {
            _contentDefinitionManager = contentDefinitionManager ?? throw new ArgumentNullException(nameof(contentDefinitionManager));
            _neo4JToOwlGeneratorService = neo4jToOwlGeneratorService ?? throw new ArgumentNullException(nameof(neo4jToOwlGeneratorService));
            _orchardToOwlGeneratorService = orchardToOwlGeneratorService ?? throw new ArgumentNullException(nameof(orchardToOwlGeneratorService));
            _visualiseGraphSyncer = visualiseGraphSyncer;
            _contentItemVersionFactory = contentItemVersionFactory;
            _nodeContentItemLookup = nodeContentItemLookup;
        }

        public ActionResult Viewer()
        {
            return View();
        }

        #pragma warning disable S4457
        //todo: params not always coming through!
        public async Task<ActionResult> Data(
            [FromQuery] string? graph,
            [FromQuery] string? contentItemId)
            //[FromQuery] string? containingContentItemId)
        {
            if (string.IsNullOrWhiteSpace(contentItemId))
                return GetOntology();

            if (string.IsNullOrEmpty(graph))
                throw new ArgumentNullException(nameof(graph));

            _contentItemVersion = _contentItemVersionFactory.Get(graph!);

            return await GetData(contentItemId, graph!);
        }
        #pragma warning restore S4457

        public async Task<ActionResult> NodeLink([FromQuery] string nodeId, [FromQuery] string route, [FromQuery] string graph)
        {
            string? contentItemId = await _nodeContentItemLookup.GetContentItemId(nodeId, graph);

            return route switch
            {
                "resetFocus" => Redirect(ResetFocusBaseUrl.Replace(ContentItemIdPlaceHolder, contentItemId).Replace(GraphPlaceHolder, graph)),
                _ => Redirect(EditBaseUrl.Replace(ContentItemIdPlaceHolder, contentItemId)),
            };
        }

        private ActionResult GetOntology()
        {
            var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions();

            var owlDataModel = _orchardToOwlGeneratorService.CreateOwlDataModels(contentTypeDefinitions);
            var owlResponseString = JsonSerializer.Serialize(owlDataModel, _jsonOptions);

            return Content(owlResponseString, MediaTypeNames.Application.Json);
        }

        private async Task<ActionResult> GetData(string contentItemId, string graphName)
        {
            var subgraph = await _visualiseGraphSyncer.GetVisualisationSubgraph(contentItemId, graphName, _contentItemVersion!);

            var owlDataModel = _neo4JToOwlGeneratorService.CreateOwlDataModels(
                subgraph.SourceNode?.Id,
                subgraph.Nodes!,
                subgraph.Relationships!,
                "skos__prefLabel");

            string owlResponseString = JsonSerializer.Serialize(owlDataModel, _jsonOptions);
            return Content(owlResponseString, MediaTypeNames.Application.Json);
        }
    }
}
