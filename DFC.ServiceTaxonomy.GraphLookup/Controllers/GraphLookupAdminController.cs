using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphLookup.Queries;
using DFC.ServiceTaxonomy.GraphLookup.Settings;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphLookup.Controllers
{
    [Admin]
    public class GraphLookupAdminController : Controller
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IGraphCluster _neoGraphCluster;

        public GraphLookupAdminController(IContentDefinitionManager contentDefinitionManager, IGraphCluster neoGraphCluster)
        {
            _contentDefinitionManager = contentDefinitionManager ?? throw new ArgumentNullException(nameof(contentDefinitionManager));
            _neoGraphCluster = neoGraphCluster ?? throw new ArgumentNullException(nameof(neoGraphCluster));
        }

        public async Task<IActionResult> SearchLookupNodes(string part, string content, string query)
        {
            if (string.IsNullOrWhiteSpace(part) || string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Part and content are required parameters");
            }

            var settings = _contentDefinitionManager
                .GetTypeDefinition(content)
                ?.Parts?.FirstOrDefault(p => p.Name == part)
                ?.GetSettings<GraphLookupPartSettings>();
            if (settings == null)
            {
                return BadRequest("Unable to find field settings");
            }

            //todo: interface and get from service provider
            //todo: add lookup graph to settings
            var results = await _neoGraphCluster.Run(GraphReplicaSetNames.Published, new LookupQuery(
                    query,
                    settings.NodeLabel!,        //todo: check can these be null (when no values entered in settings)?
                    settings.DisplayFieldName!,
                    settings.ValueFieldName!));

            return new ObjectResult(results);
        }
    }
}
