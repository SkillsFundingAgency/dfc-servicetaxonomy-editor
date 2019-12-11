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

    [Admin]
    public class GraphLookupAdminController : Controller
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly INeoGraphDatabase _neoGraphDatabase;

        public GraphLookupAdminController(IContentDefinitionManager contentDefinitionManager, INeoGraphDatabase neoGraphDatabase)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _neoGraphDatabase = neoGraphDatabase;
        }

        public async Task<IActionResult> SearchLookupNodes(string part, string content, string query)
        {
            if (string.IsNullOrWhiteSpace(part))
            {
                return BadRequest("Part are required parameters");
            }

            //todo: pass in type name
            var settings = _contentDefinitionManager
                .GetTypeDefinition(content)
                .Parts.FirstOrDefault(p => p.Name == part)
                .GetSettings<GraphLookupPartSettings>();
            if (settings == null)
            {
                return BadRequest("Unable to find field settings");
            }

            try
            {
                //todo: rename to IdFieldName, as needs to be unique??
                //todo: assumes array of display fields
                var results = await _neoGraphDatabase.RunReadStatement(
                    //todo: hardcode as names
                    new Statement(
$@"match (n:{settings.NodeLabel})
where head(n.{settings.DisplayFieldName}) starts with '{query}'
return head(n.
{settings.DisplayFieldName}) as {settings.DisplayFieldName}, n.{settings.ValueFieldName} as {settings.ValueFieldName}
order by {settings.DisplayFieldName}
limit 50"),
                    r => new VueMultiselectItemViewModel { Id = r[settings.ValueFieldName].ToString(), DisplayText = r[settings.DisplayFieldName].ToString() });

                return new ObjectResult(results);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
#pragma warning restore S2479 // Whitespace and control characters in string literals should be explicit
}
