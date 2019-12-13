using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Neo4j.Services;
using DFC.ServiceTaxonomy.Editor.Module.Settings;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using OrchardCore.Admin;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.Editor.Module.Controllers
{
#pragma warning disable S2479 // Whitespace and control characters in string literals should be explicit
//todo: initial load doesn't look like the first 50 ordered!
    [Admin]
    public class GraphLookupAdminController : Controller
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly INeoGraphDatabase _neoGraphDatabase;

        public GraphLookupAdminController(IContentDefinitionManager contentDefinitionManager, INeoGraphDatabase neoGraphDatabase)
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

            //todo: rename to IdFieldName, as needs to be unique??
            //todo: assumes array of display fields

            const string displayField = "d";
            const string valueField = "v";

            var results = await _neoGraphDatabase.RunReadStatement(
                new Statement(
$@"match (n:{settings.NodeLabel})
where head(n.{settings.DisplayFieldName}) starts with '{query}'
return head(n.{settings.DisplayFieldName}) as {displayField}, n.{settings.ValueFieldName} as {valueField}
order by {displayField}
limit 50"),
                r => new VueMultiselectItemViewModel { Id = r[valueField].ToString(), DisplayText = r[displayField].ToString() });

            return new ObjectResult(results);
        }
    }
#pragma warning restore S2479 // Whitespace and control characters in string literals should be explicit
}
