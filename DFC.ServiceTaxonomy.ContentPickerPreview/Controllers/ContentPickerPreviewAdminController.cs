using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentPickerPreview.Models;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.Controllers
{
    [Admin]
    public class ContentPickerPreviewAdminController : Controller
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentPickerResultProvider> _resultProviders;

        public ContentPickerPreviewAdminController(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentPickerResultProvider> resultProviders)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _resultProviders = resultProviders;
        }

        public async Task<IActionResult> SearchContentItems(string part, string query)
        {
            if (string.IsNullOrWhiteSpace(part))
            {
                return BadRequest("Part is a required parameter");
            }

            var partDefinition = _contentDefinitionManager.GetPartDefinition(part);

            var partSettings = partDefinition?.GetSettings<ContentPickerPreviewPartSettings>();
            if (partSettings == null)
            {
                return BadRequest("Unable to find part settings");
            }

            //todo:
            //string editor = partFieldDefinition.Editor() ?? "Default";
            string editor = "Default";
            var resultProvider = _resultProviders.FirstOrDefault(p => p.Name == editor);
            if (resultProvider == null)
            {
                return new ObjectResult(new List<ContentPickerResult>());
            }

            // probably gonna need a new SearchProvider, or bypass it, as we don't have a partfielddefinition to pass
            // think we're in luck, DefaultContentPickerResultProvider doesn't use PartFieldDefinition
            var results = await resultProvider.Search(new ContentPickerSearchContext
            {
                Query = query,
                ContentTypes = partSettings.DisplayedContentTypes
            });

            return new ObjectResult(results.Select(r => new VueMultiselectItemViewModel { Id = r.ContentItemId, DisplayText = r.DisplayText, HasPublished = r.HasPublished }));
        }
    }
}
