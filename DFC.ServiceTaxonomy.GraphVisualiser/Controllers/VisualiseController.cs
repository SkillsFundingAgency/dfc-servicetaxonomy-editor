using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Indexes;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries;
using DFC.ServiceTaxonomy.GraphVisualiser.Services;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using YesSql;

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
        private readonly ISuperpositionContentItemVersion _superpositionContentItemVersion;
        private readonly ISyncNameProvider _syncNameProvider;
        private readonly ISession _session;
        private readonly IContentItemVersionFactory _contentItemVersionFactory;
        private IContentItemVersion? _contentItemVersion;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public VisualiseController(
            IGraphCluster neoGraphCluster,
            IContentDefinitionManager contentDefinitionManager,
            INeo4JToOwlGeneratorService neo4jToOwlGeneratorService,
            ISyncNameProvider syncNameProvider,
            IOrchardToOwlGeneratorService orchardToOwlGeneratorService,
            IVisualiseGraphSyncer visualiseGraphSyncer,
            ISuperpositionContentItemVersion superpositionContentItemVersion, ISession session, IContentItemVersionFactory contentItemVersionFactory)
        {
            _neoGraphCluster = neoGraphCluster ?? throw new ArgumentNullException(nameof(neoGraphCluster));
            _contentDefinitionManager = contentDefinitionManager ?? throw new ArgumentNullException(nameof(contentDefinitionManager));
            _neo4JToOwlGeneratorService = neo4jToOwlGeneratorService ?? throw new ArgumentNullException(nameof(neo4jToOwlGeneratorService));
            _orchardToOwlGeneratorService = orchardToOwlGeneratorService ?? throw new ArgumentNullException(nameof(orchardToOwlGeneratorService));
            _visualiseGraphSyncer = visualiseGraphSyncer;
            _superpositionContentItemVersion = superpositionContentItemVersion;
            _session = session;
            _contentItemVersionFactory = contentItemVersionFactory;
            _syncNameProvider = syncNameProvider;
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
            _contentItemVersion = _contentItemVersionFactory.Get(graph);

            var graphSyncNodeId = _syncNameProvider.IdPropertyValueFromNodeValue(nodeId, _contentItemVersion, _superpositionContentItemVersion);

            var contentItems = await _session.Query<ContentItem, GraphSyncPartIndex>(x => x.NodeId == graphSyncNodeId).ListAsync();

            return route switch
            {
                "resetFocus" => Redirect(ResetFocusBaseUrl.Replace(ContentItemIdPlaceHolder, contentItems?.FirstOrDefault()?.ContentItemId).Replace(GraphPlaceHolder, graph)),
                _ => Redirect(EditBaseUrl.Replace(ContentItemIdPlaceHolder, contentItems?.FirstOrDefault()?.ContentItemId)),
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
