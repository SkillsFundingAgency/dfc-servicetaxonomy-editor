using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.GraphVisualiser.Services;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
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
        private readonly IGraphCluster _neoGraphCluster;
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
            IGraphCluster neoGraphCluster,
            IContentDefinitionManager contentDefinitionManager,
            INeo4JToOwlGeneratorService neo4jToOwlGeneratorService,
            IOrchardToOwlGeneratorService orchardToOwlGeneratorService,
            IVisualiseGraphSyncer visualiseGraphSyncer,
            IContentItemVersionFactory contentItemVersionFactory,
            INodeContentItemLookup nodeContentItemLookup)
        {
            _neoGraphCluster = neoGraphCluster ?? throw new ArgumentNullException(nameof(neoGraphCluster));
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

        public async Task<ActionResult> Data([FromQuery] string? contentItemId, [FromQuery] string? graph)
        {
            ValidateParameters(graph);

            if (string.IsNullOrWhiteSpace(contentItemId))
            {
                return GetOntology();
            }

            _contentItemVersion = _contentItemVersionFactory.Get(graph!);

            return await GetData(contentItemId, graph!);
        }

        public async Task<ActionResult> NodeLink([FromQuery] string nodeId, [FromQuery] string route, [FromQuery] string graph)
        {
            string? contentItemId = await _nodeContentItemLookup.GetContentItemId(nodeId, graph);

            return route switch
            {
                "resetFocus" => Redirect(ResetFocusBaseUrl.Replace(ContentItemIdPlaceHolder, contentItemId).Replace(GraphPlaceHolder, graph)),
                _ => Redirect(EditBaseUrl.Replace(ContentItemIdPlaceHolder, contentItemId)),
            };
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

        private async Task<ActionResult> GetData(string contentItemId, string graph)
        {
            var relationshipCommands = await _visualiseGraphSyncer.BuildVisualisationCommands(contentItemId, _contentItemVersion!);
            var result = await _neoGraphCluster.Run(graph, relationshipCommands.ToArray());

            string owlResponseString = "";
            IEnumerable<INode> nodesToProcess = new List<INode>();
            long sourceNodeId = 0;
            HashSet<IRelationship> relationships = new HashSet<IRelationship>();

            if (result.Any())
            {
                //Get all outgoing relationships from the query and add in any source nodes
                nodesToProcess = result.SelectMany(x => x!.OutgoingRelationships.Select(x => x.outgoingRelationship.DestinationNode)).Union(result.GroupBy(x => x!.SourceNode).Select(z => z.FirstOrDefault()!.SourceNode));
                sourceNodeId = result.FirstOrDefault()!.SourceNode.Id;
                relationships = result!.SelectMany(y => y!.OutgoingRelationships.Select(z => z.outgoingRelationship.Relationship)).ToHashSet<IRelationship>();
            }
            else
            {
                var nodeOnlyResult = await _neoGraphCluster.Run(graph, new NodeWithOutgoingRelationshipsQuery(_visualiseGraphSyncer.SourceNodeLabels!, _visualiseGraphSyncer.SourceNodeIdPropertyName!, _visualiseGraphSyncer.SourceNodeId!));
                nodesToProcess = nodeOnlyResult.GroupBy(x => x!.SourceNode).Select(z => z.FirstOrDefault()!.SourceNode);
                sourceNodeId = nodeOnlyResult.FirstOrDefault()!.SourceNode.Id;
                relationships = nodeOnlyResult!.SelectMany(y => y!.OutgoingRelationships.Select(z => z.Relationship)).ToHashSet<IRelationship>();
            }

            var owlDataModel = _neo4JToOwlGeneratorService.CreateOwlDataModels(sourceNodeId, nodesToProcess, relationships, "skos__prefLabel");
            owlResponseString = JsonSerializer.Serialize(owlDataModel, _jsonOptions);
            return Content(owlResponseString, MediaTypeNames.Application.Json);
        }
    }
}
