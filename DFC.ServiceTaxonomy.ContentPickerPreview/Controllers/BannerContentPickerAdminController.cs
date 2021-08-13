using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentPickerPreview.Models;
using DFC.ServiceTaxonomy.ContentPickerPreview.Services;
using DFC.ServiceTaxonomy.ContentPickerPreview.ViewModels;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.Controllers
{
    [Admin]
    public class BannerContentPickerAdminController : Controller
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IBannerContentPickerResultProvider> _resultProviders;

        public BannerContentPickerAdminController(IContentDefinitionManager contentDefinitionManager, IEnumerable<IBannerContentPickerResultProvider> resultProviders)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _resultProviders = resultProviders;
        }

        public async Task<IActionResult> SearchContentItems(string part, string field, string query)
        {
            if (string.IsNullOrWhiteSpace(part) || string.IsNullOrWhiteSpace(field))
            {
                return BadRequest("Part and field are required parameters");
            }

            ContentPartFieldDefinition? partFieldDefinition = _contentDefinitionManager.GetPartDefinition(part)?.Fields.FirstOrDefault(f => f.Name == field);

            ContentPickerFieldSettings? fieldSettings = partFieldDefinition?.GetSettings<ContentPickerFieldSettings>();

            if (fieldSettings == null)
            {
                return BadRequest("Unable to find field definition");
            }

            string? editor = partFieldDefinition?.GetSettings<ContentPartFieldSettings>().Editor ?? "Default";
            IBannerContentPickerResultProvider? resultProvider = _resultProviders.FirstOrDefault(p => p.Name == editor);

            if (resultProvider == null)
            {
                return new ObjectResult(new List<ContentPickerResult>());
            }

            IEnumerable<BannerContentPickerResult>? results = await resultProvider.Search(new ContentPickerSearchContext
            {
                Query = query,
                DisplayAllContentTypes = fieldSettings.DisplayAllContentTypes,
                ContentTypes = fieldSettings.DisplayedContentTypes,
                PartFieldDefinition = partFieldDefinition
            });

            return new ObjectResult(results.Select(r =>
                        new VueMultiselectBannerItemViewModel()
                        {
                            Id = r.ContentItemId,
                            DisplayText = r.DisplayText,
                            HasPublished = r.HasPublished,
                            IsActive = r.IsActive
                        }));
        }
    }
}
