﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DFC.ServiceTaxonomy.VersionComparison.Models;
using DFC.ServiceTaxonomy.VersionComparison.Services;
using DFC.ServiceTaxonomy.VersionComparison.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;

using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.VersionComparison.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuditTrailQueryService _auditTrailQueryService;
        private readonly IDiffBuilderService _diffBuilderService;
        private readonly IDisplayManager<DiffItem> _diffItemDisplayManager;
        private readonly IDisplayManager<VersionComparisonOptions> _versionComparisonOptionsDisplayManager;
        private readonly IHtmlLocalizer<AdminController> _h;
        private readonly INotifier _notifier;
        private readonly IShapeFactory _shapeFactory;
        private readonly IUpdateModelAccessor _updateModelAccessor;

        public AdminController(
            IAuditTrailQueryService auditTrailQueryService,
            IDiffBuilderService diffBuilderService,
            IDisplayManager<DiffItem> diffItemDisplayManager,
            IDisplayManager<VersionComparisonOptions> versionComparisonOptionsDisplayManager,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            INotifier notifier,
            IShapeFactory shapeFactory,
            IUpdateModelAccessor updateModelAccessor
        )
        {
            _auditTrailQueryService = auditTrailQueryService;
            _diffBuilderService = diffBuilderService;
            _diffItemDisplayManager = diffItemDisplayManager;
            _versionComparisonOptionsDisplayManager = versionComparisonOptionsDisplayManager;
            _h = htmlLocalizer;
            _notifier = notifier;
            _shapeFactory = shapeFactory;
            _updateModelAccessor = updateModelAccessor;
        }

        [HttpGet]
        public async Task<ActionResult> Index(string contentItemId)
        {

            var versions = await _auditTrailQueryService.GetVersions(contentItemId);
            if (!versions.Any())
            {
                return NotFound();
            }
            if (versions.Count == 1)
            {
                await _notifier.WarningAsync(_h["There is currently only one version of this content item."]);
            }

            var shapeViewModel = await GetShapeViewModel(new VersionComparisonOptions { ContentItemId = contentItemId }, versions);

            return View(shapeViewModel);
        }

        [HttpPost, ActionName("Index")]
        public async Task<ActionResult> IndexPost([Bind(Prefix = "VersionComparisonOptions")] VersionComparisonOptions options)
        {
            if (options.BaseVersion == options.CompareVersion)
            {
                await _notifier.WarningAsync(_h["Both the base and compare version are the same."]);
            }

            var versions = await _auditTrailQueryService.GetVersions(options.ContentItemId ?? string.Empty);

            var shapeViewModel = await GetShapeViewModel(options, versions);

            return View(shapeViewModel);
        }

        private async Task<IShape> GetShapeViewModel(VersionComparisonOptions options, List<AuditTrailContentEvent> versions)
        {
            BuildOptions(options, versions);

            var selectListShape =
                await _versionComparisonOptionsDisplayManager.BuildEditorAsync(options,
                    _updateModelAccessor.ModelUpdater, false, "", "");

            var items = new List<IShape>();
            var diffItems = _diffBuilderService.BuildDiffList(versions.First(v => v.VersionNumber.ToString() == options.BaseVersion).ContentItem,
                versions.First(v => v.VersionNumber.ToString() == options.CompareVersion).ContentItem);
            foreach (DiffItem diffItem in diffItems)
            {
                items.Add(await _diffItemDisplayManager.BuildDisplayAsync(diffItem, _updateModelAccessor.ModelUpdater, "SummaryAdmin"));
            }

            var contentItem = versions.First().ContentItem;

            return await _shapeFactory.CreateAsync<VersionComparisonViewModel>("VersionComparison",
                viewModel =>
                {
                    viewModel.ContentItemId = options.ContentItemId;
                    viewModel.ContentItemDisplayName = contentItem.DisplayText;
                    viewModel.ContentItemContentType = contentItem.ContentType;
                    viewModel.SelectLists = selectListShape;
                    viewModel.DiffItems = items;
                });
        }

        private static void BuildOptions(VersionComparisonOptions options, List<AuditTrailContentEvent> versions)
        {
            var selectVersions = BuildSelectList(versions.Select(v => v.VersionNumber).ToList());
            options.BaseVersionSelectListItems = selectVersions;
            options.CompareVersionSelectListItems = selectVersions;
            if (string.IsNullOrWhiteSpace(options.BaseVersion) || string.IsNullOrWhiteSpace(options.CompareVersion))
            {
                options.BaseVersion = selectVersions[0].Value;
                options.CompareVersion = versions.Count > 1 ? selectVersions[1].Value : selectVersions[0].Value;
            }
        }

        private static List<SelectListItem> BuildSelectList(List<int> versionNumbers)
        {
            var selectListItems = new List<SelectListItem>(versionNumbers.Count);
            for (int i = 0; i < versionNumbers.Count; i++)
            {
                var versionNumberValue = versionNumbers[i].ToString();
                var versionNumberText = $"version {versionNumberValue}{(i == 0 ? " (latest)" : string.Empty)}";
                selectListItems.Add(new SelectListItem(versionNumberText, versionNumberValue));
            }
            return selectListItems;
        }
    }
}
