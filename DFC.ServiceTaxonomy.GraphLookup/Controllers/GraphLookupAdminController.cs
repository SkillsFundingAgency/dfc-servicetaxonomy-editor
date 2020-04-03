using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphLookup.Queries;
using DFC.ServiceTaxonomy.GraphLookup.Settings;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphLookup.Controllers
{
    [Admin]
    public class GraphLookupAdminController : Controller
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IGraphDatabase _neoGraphDatabase;

        public GraphLookupAdminController(IContentDefinitionManager contentDefinitionManager, IGraphDatabase neoGraphDatabase)
        {
            _contentDefinitionManager = contentDefinitionManager ?? throw new ArgumentNullException(nameof(contentDefinitionManager));
            _neoGraphDatabase = neoGraphDatabase ?? throw new ArgumentNullException(nameof(neoGraphDatabase));
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
            var results = await _neoGraphDatabase.Run(new LookupQuery(
                    query,
                    settings.NodeLabel!,        //todo: check can these be null (when no values entered in settings)?
                    settings.DisplayFieldName!,
                    settings.ValueFieldName!));

            return new ObjectResult(results);
        }
    }
}
