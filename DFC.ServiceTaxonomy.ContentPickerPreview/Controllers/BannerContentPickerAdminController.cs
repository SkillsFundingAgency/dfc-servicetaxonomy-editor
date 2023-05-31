﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentPickerPreview.Models;
using DFC.ServiceTaxonomy.ContentPickerPreview.Services;
using DFC.ServiceTaxonomy.ContentPickerPreview.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.Controllers
{
    [Admin]
    public class BannerContentPickerAdminController : Controller
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IBannerContentPickerResultProvider> _resultProviders;
        private readonly ILogger _logger;

        public BannerContentPickerAdminController(IContentDefinitionManager contentDefinitionManager, IEnumerable<IBannerContentPickerResultProvider> resultProviders,ILogger logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _resultProviders = resultProviders;
            _logger = logger;
        }

        public async Task<IActionResult> SearchContentItems(string part, string field, string query)
        {
            if (string.IsNullOrWhiteSpace(part) || string.IsNullOrWhiteSpace(field))
            {
                _logger.LogError($"SearchContentItems Part {part} and field {field} query {query} are required parameters");
                return BadRequest("Part and field are required parameters");
            }

            ContentPartFieldDefinition? partFieldDefinition = _contentDefinitionManager.GetPartDefinition(part)?.Fields.FirstOrDefault(f => f.Name == field);

            ContentPickerFieldSettings? fieldSettings = partFieldDefinition?.GetSettings<ContentPickerFieldSettings>();

            if (fieldSettings == null)
            {
                _logger.LogError($"SearchContentItems Part {part} and field {field} query {query} Unable to find field definition");
                return BadRequest("Unable to find field definition");
            }

            string? editor = partFieldDefinition.Editor() ?? "Default";
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
