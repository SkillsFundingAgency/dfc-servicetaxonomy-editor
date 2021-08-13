using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using DFC.ServiceTaxonomy.VersionComparison.Services;
using DFC.ServiceTaxonomy.VersionComparison.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.VersionComparison.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuditTrailQueryService _auditTrailQueryService;
        private readonly IContentManager _contentManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly IDisplayManager<VersionComparisonOptions> _versionComparisonOptionsDisplayManager;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<AdminController> _h;

        public AdminController(
            IAuditTrailQueryService auditTrailQueryService,
            IContentManager contentManager,
            IDisplayManager<VersionComparisonOptions> versionComparisonOptionsDisplayManager,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            INotifier notifier,
            IShapeFactory shapeFactory,
            IUpdateModelAccessor updateModelAccessor
        )
        {
            _auditTrailQueryService = auditTrailQueryService;
            _contentManager = contentManager;
            _h = htmlLocalizer;
            _notifier = notifier;
            _shapeFactory = shapeFactory;
            _updateModelAccessor = updateModelAccessor;
            _versionComparisonOptionsDisplayManager = versionComparisonOptionsDisplayManager;
        }

        public async Task<ActionResult> Index(string contentItemId)
        {

            var versions = await _auditTrailQueryService.GetVersions(contentItemId);
            if (!versions.Any())
            {
                return NotFound();
            }
            if (versions.Count == 1)
            {
                _notifier.Warning(_h["There is currently only one version of this content item."]);
            }

            var contentItem = versions.First().ContentItem;

            var selectVersions = BuildSelectList(versions.Select(v => v.VersionNumber).ToList());

            var options = new VersionComparisonOptions
            {
                ContentItemId = contentItemId,
                BaseVersion = selectVersions[0].Value,
                BaseVersionSelectListItems = selectVersions,
                CompareVersion = versions.Count > 1 ? selectVersions[1].Value : selectVersions[0].Value,
                CompareVersionSelectListItems = selectVersions
            };

            var shapeViewModel = await GetShapeViewModel(options, contentItem);

            return View(shapeViewModel);
        }

        [HttpPost, ActionName("Index")]
        public async Task<ActionResult> IndexPost([Bind(Prefix = "VersionComparisonOptions")] VersionComparisonOptions options)
        {
            if (options.BaseVersion == options.CompareVersion)
            {
                _notifier.Warning(_h["Both the base and compare version are the same."]);
            }

            var versions = await _auditTrailQueryService.GetVersions(options.ContentItemId ?? string.Empty);
            var contentItem = versions.First().ContentItem;
            var selectVersions = BuildSelectList(versions.Select(v => v.VersionNumber).ToList());

            options.BaseVersionSelectListItems = selectVersions;
            options.CompareVersionSelectListItems = selectVersions;

            var shapeViewModel = await GetShapeViewModel(options, contentItem);

            return View(shapeViewModel);
        }

        private async Task<IShape> GetShapeViewModel(VersionComparisonOptions options, ContentItem contentItem)
        {
            var selectListShape =
                await _versionComparisonOptionsDisplayManager.BuildEditorAsync(options,
                    _updateModelAccessor.ModelUpdater, false);


            return await _shapeFactory.CreateAsync<VersionComparisonViewModel>("VersionComparison",
                viewModel =>
                {
                    viewModel.ContentItemId = options.ContentItemId;
                    viewModel.ContentItemDisplayName = contentItem.DisplayText;
                    viewModel.ContentItemContentType = contentItem.ContentType;
                    viewModel.SelectLists = selectListShape;
                });
        }

        private List<SelectListItem> BuildSelectList(List<int> versionNumbers)
        {
            var selectListItems = new List<SelectListItem>();
            for (int i = 0; i < versionNumbers.Count; i++)
            {
                var versionNumberValue = versionNumbers[i].ToString();
                var versionNumberText = string.Format("version {0}{1}", versionNumberValue,
                    i == 0 ? " (latest)" : string.Empty);
                selectListItems.Add(new SelectListItem(versionNumberText, versionNumberValue));
            }
            return selectListItems;
        }
    }
}
