using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Settings;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.Editor.Module.Controllers
{
    [Admin]
    public class GraphLookupAdminController : Controller
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentPickerResultProvider> _resultProviders;

        public GraphLookupAdminController(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentPickerResultProvider> resultProviders
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _resultProviders = resultProviders;
        }

        public Task<IActionResult> SearchLookupNodes(string part, string field, string query)
        {
            if (string.IsNullOrWhiteSpace(part) || string.IsNullOrWhiteSpace(field))
            {
                return Task.FromResult((IActionResult)BadRequest("Part and field are required parameters"));
            }

            var partFieldDefinition = _contentDefinitionManager.GetPartDefinition(part)?.Fields
                .FirstOrDefault(f => f.Name == field);

            var fieldSettings = partFieldDefinition?.GetSettings<GraphLookupFieldSettings>();
            if (fieldSettings == null)
            {
                return Task.FromResult((IActionResult)BadRequest("Unable to find field definition"));
            }

            var editor = partFieldDefinition.Editor() ?? "Default";
            var resultProvider = _resultProviders.FirstOrDefault(p => p.Name == editor);
            if (resultProvider == null)
            {
                return Task.FromResult((IActionResult)new ObjectResult(new List<ContentPickerResult>()));
            }

            //var results = await resultProvider.Search(new ContentPickerSearchContext
            //{
            //    Query = query,
            //    ContentTypes = fieldSettings.DisplayedContentTypes,
            //    PartFieldDefinition = partFieldDefinition
            //});

            var results = new[]
            {
                new { ContentItemId = "Whistle", DisplayText = "http://esco/skill/123" },
                new { ContentItemId = "Juggle", DisplayText = "http://esco/skill/456" }
            };

            return Task.FromResult((IActionResult)new ObjectResult(results.Select(r => new VueMultiselectItemViewModel { Id = r.ContentItemId, DisplayText = r.DisplayText })));
        }
    }
}
