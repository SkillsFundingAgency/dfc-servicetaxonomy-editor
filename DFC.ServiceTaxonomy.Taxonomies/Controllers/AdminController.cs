using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Taxonomies.Handlers;
using DFC.ServiceTaxonomy.Taxonomies.Helper;
using DFC.ServiceTaxonomy.Taxonomies.Models;
using DFC.ServiceTaxonomy.Taxonomies.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Title.Models;
using YesSql;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.Taxonomies.Controllers
{
    public class AdminController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISession _session;
        private readonly IHtmlLocalizer H;
        private readonly INotifier _notifier;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IEnumerable<ITaxonomyTermValidator> _validators;
        private readonly IEnumerable<ITaxonomyTermHandler> _handlers;
        private readonly ITaxonomyHelper _taxonomyHelper;
        private readonly ILogger _logger;

        public AdminController(
            ISession session,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            IContentItemDisplayManager contentItemDisplayManager,
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier,
            IHtmlLocalizer<AdminController> localizer,
            IUpdateModelAccessor updateModelAccessor,
            IEnumerable<ITaxonomyTermValidator> validators,
            IEnumerable<ITaxonomyTermHandler> handlers,
            ITaxonomyHelper taxonomyHelper,ILogger logger)
        {
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentDefinitionManager = contentDefinitionManager;
            _session = session;
            _notifier = notifier;
            _updateModelAccessor = updateModelAccessor;
            H = localizer;
            _validators = validators;
            _handlers = handlers;
            _taxonomyHelper = taxonomyHelper;
            _logger = logger;
        }

        public async Task<IActionResult> Create(string id, string taxonomyContentItemId, string taxonomyItemId)
        {
            _logger.LogInformation($"Create id {id}, taxonomyContentItemId {taxonomyContentItemId} taxonomyItemId {taxonomyItemId} ");
            
            if (String.IsNullOrWhiteSpace(id))
            {
                _logger.LogInformation($"Create id {id} Notfound ");
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies))
            {
                _logger.LogInformation($"Create User forbidden ");
                return Forbid();
            }

            var contentItem = await _contentManager.NewAsync(id);
            contentItem.Weld<TermPart>();
            contentItem.Alter<TermPart>(t => t.TaxonomyContentItemId = taxonomyContentItemId);

            dynamic model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true);

            model.TaxonomyContentItemId = taxonomyContentItemId;
            model.TaxonomyItemId = taxonomyItemId;

            return View(model);
        }

        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(string id, string taxonomyContentItemId, string taxonomyItemId)
        {
            _logger.LogInformation($"CreatePost id {id}, taxonomyContentItemId {taxonomyContentItemId} taxonomyItemId {taxonomyItemId} ");
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies))
            {
                _logger.LogInformation($"Create User forbidden ");
                return Forbid();
            }

            ContentItem taxonomy;

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition("Taxonomy");

            if (!contentTypeDefinition.GetSettings<ContentTypeSettings>().Draftable)
            {
                taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.Latest);
            }
            else
            {
                taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.DraftRequired);
            }

            if (taxonomy == null)
            {
                _logger.LogInformation($"taxonomy {taxonomy} Notfound");
                return NotFound();
            }

            var contentItem = await _contentManager.NewAsync(id);
            contentItem.Published = true;
            contentItem.Weld<TermPart>();
            contentItem.Alter<TermPart>(t => t.TaxonomyContentItemId = taxonomyContentItemId);

            dynamic model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true);

            if (!ModelState.IsValid)
            {
                model.TaxonomyContentItemId = taxonomyContentItemId;
                model.TaxonomyItemId = taxonomyItemId;

                _logger.LogInformation($"ModelState not valid model.TaxonomyContentItemId {model.TaxonomyContentItemId}" );

                return View(model);
            }

            if (taxonomyItemId == null)
            {
                // Use the taxonomy as the parent if no target is specified
                if (!ValidateTaxonomyTerm(taxonomy, contentItem))
                {

                    _logger.LogError($"Another {contentItem.ContentType} already exists with this name.");

                    return ValidationError($"Another {contentItem.ContentType} already exists with this name.", model, taxonomyContentItemId, taxonomyItemId);
                }

                taxonomy.Alter<TaxonomyPart>(part => part.Terms.Add(contentItem));
            }
            else
            {
                // Look for the target taxonomy item in the hierarchy
                var parentTaxonomyItem = FindTaxonomyItem(taxonomy.As<TaxonomyPart>().Content, taxonomyItemId);

                // Couldn't find targeted taxonomy item
                if (parentTaxonomyItem == null)
                {
                    _logger.LogError($"parentTaxonomyItem {parentTaxonomyItem} NotFound.");
                    return NotFound();
                }

                if (!ValidateTaxonomyTerm(parentTaxonomyItem.ToObject<ContentItem>(), contentItem))
                {
                    _logger.LogError($"Another {contentItem.ContentType} already exists with this name.");

                    return ValidationError($"Another {contentItem.ContentType} already exists with this name.", model, taxonomyContentItemId, taxonomyItemId);
                }

                var taxonomyItems = parentTaxonomyItem?.Terms as JArray;

                if (taxonomyItems == null)
                {
                    parentTaxonomyItem["Terms"] = taxonomyItems = new JArray();
                }

                taxonomyItems.Add(JObject.FromObject(contentItem));
            }

            foreach (var validator in _validators)
            {
                (bool validated, string errorMessage) =
                    await validator.ValidateCreate(JObject.FromObject(contentItem), JObject.FromObject(taxonomy));

                if (!validated)
                {
                    _logger.LogError($"validator {taxonomyContentItemId}  errorMessage {errorMessage}");
                    return ValidationError(errorMessage, model, taxonomyContentItemId, taxonomyItemId);
                }
            }

            taxonomy.Published = false;
            await _contentManager.PublishAsync(taxonomy);

            //Content item will get published as part of the taxonomy, handler ensure Event Grid is informed of Content Item change
            foreach (var handler in _handlers)
            {
                await handler.PublishedAsync(contentItem);
            }

            return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = taxonomyContentItemId });
        }

        public async Task<IActionResult> Edit(string taxonomyContentItemId, string taxonomyItemId)
        {
            var taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.Latest);

            if (taxonomy == null)
            {
                _logger.LogError($"Edit taxonomy  {taxonomy} notfound");
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies, taxonomy))
            {
                _logger.LogError($"AuthorizeAsync forbid");
                return Forbid();
            }

            // Look for the target taxonomy item in the hierarchy
            JObject taxonomyItem = FindTaxonomyItem(taxonomy.As<TaxonomyPart>().Content, taxonomyItemId);

            // Couldn't find targeted taxonomy item
            if (taxonomyItem == null)
            {
                _logger.LogError($"taxonomyItem {taxonomyItem} notfound");
                return NotFound();
            }

            var contentItem = taxonomyItem.ToObject<ContentItem>();
            contentItem.Weld<TermPart>();
            contentItem.Alter<TermPart>(t => t.TaxonomyContentItemId = taxonomyContentItemId);

            dynamic model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

            model.TaxonomyContentItemId = taxonomyContentItemId;
            model.TaxonomyItemId = taxonomyItemId;

            return View(model);
        }

        [HttpPost]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost(string taxonomyContentItemId, string taxonomyItemId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies))
            {
                _logger.LogError($"EditPost taxonomyContentItemId{taxonomyContentItemId} forbid");
                return Forbid();
            }

            ContentItem taxonomy;

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition("Taxonomy");

            if (!contentTypeDefinition.GetSettings<ContentTypeSettings>().Draftable)
            {
                taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.Latest);
            }
            else
            {
                taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.DraftRequired);
            }

            if (taxonomy == null)
            {
                _logger.LogError($"EditPost taxonomy {taxonomy} notfound");
                return NotFound();
            }

            // Look for the target taxonomy item in the hierarchy
            JObject taxonomyItem = FindTaxonomyItem(taxonomy.As<TaxonomyPart>().Content, taxonomyItemId);

            // Couldn't find targeted taxonomy item
            if (taxonomyItem == null)
            {
                _logger.LogError($"EditPost taxonomyItem {taxonomyItem} notfound");
                return NotFound();
            }

            var existing = taxonomyItem.ToObject<ContentItem>();

            // Create a new item to take into account the current type definition.
            var contentItem = await _contentManager.NewAsync(existing.ContentType);

            contentItem.Merge(existing);            
            contentItem.Weld<TermPart>();
            contentItem.Alter<TermPart>(t => t.TaxonomyContentItemId = taxonomyContentItemId);
            contentItem.ContentItemId = existing.ContentItemId;

            dynamic model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

            if (!ModelState.IsValid)
            {
                model.TaxonomyContentItemId = taxonomyContentItemId;
                model.TaxonomyItemId = taxonomyItemId;
                _logger.LogInformation($"EditPost TaxonomyContentItemId {taxonomyContentItemId} ");
                return View(model);
            }

            //when editing we don't know what the parent id is, so we have to search for it
            var parentTaxonomyTerm = _taxonomyHelper.FindParentTaxonomyTerm(JObject.FromObject(contentItem), JObject.FromObject(taxonomy));

            if (parentTaxonomyTerm == null)
                return NotFound();

            if (!ValidateTaxonomyTerm(parentTaxonomyTerm, contentItem))
            {
                _logger.LogInformation($"Another {contentItem.ContentType} already exists with this name.");
                return ValidationError($"Another {contentItem.ContentType} already exists with this name.", model, taxonomyContentItemId, taxonomyItemId);
            }

            taxonomyItem.Merge(contentItem.Content, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Merge
            });

            // Merge doesn't copy the properties
            taxonomyItem[nameof(ContentItem.DisplayText)] = contentItem.DisplayText;

            foreach (var validator in _validators)
            {
                if(validator.GetType().Name == "PageLocationModificationValidator" &&
                    existing.As<TitlePart>().Title == contentItem.As<TitlePart>().Title)
                {
                    continue;
                }

                (bool validated, string errorMessage) = await validator.ValidateUpdate(JObject.FromObject(contentItem),
                    JObject.FromObject(taxonomy));

                if (!validated)
                {
                    return ValidationError(errorMessage, model, taxonomyContentItemId, taxonomyItemId);
                }
            }

            taxonomy.Published = false;
            await _contentManager.PublishAsync(taxonomy);

            //Content item will get published as part of the taxonomy, handler ensure Event Grid is informed of Content Item change
            foreach (var handler in _handlers)
            {
                var updated = await handler.UpdatedAsync(contentItem, taxonomy);

                if (updated)
                {
                    await handler.PublishedAsync(contentItem);
                }
            }

            return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = taxonomyContentItemId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string taxonomyContentItemId, string taxonomyItemId)
        {
            _logger.LogInformation($"Delete taxonomyContentItemId {taxonomyContentItemId} taxonomyItemId {taxonomyItemId}");
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies))
            {
                
                    _logger.LogInformation($"Delete user permission forbid");
                    return Forbid();
            }

            ContentItem taxonomy;

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition("Taxonomy");

            if (!contentTypeDefinition.GetSettings<ContentTypeSettings>().Draftable)
            {
                taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.Latest);
            }
            else
            {
                taxonomy = await _contentManager.GetAsync(taxonomyContentItemId, VersionOptions.DraftRequired);
            }

            if (taxonomy == null)
            {
                _logger.LogInformation($"Delete taxonomy {taxonomy} notfound");
                return NotFound();
            }

            // Look for the target taxonomy item in the hierarchy
            var taxonomyItem = FindTaxonomyItem(taxonomy.As<TaxonomyPart>().Content, taxonomyItemId);

            // Couldn't find targeted taxonomy item
            if (taxonomyItem == null)
            {
                _logger.LogInformation($"Delete taxonomyItem {taxonomyItem} notfound");
                return NotFound();
            }

            foreach (var validator in _validators)
            {
                (bool validated, string errorMessage) =
                    ((bool validated, string errorMessage))await validator.ValidateDelete(taxonomyItem, JObject.FromObject(taxonomy));

                if (!validated)
                {
                    await _notifier.ErrorAsync(H[errorMessage]);
                    return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = taxonomyContentItemId });
                }
            }

            taxonomyItem.Remove();
            taxonomy.Published = false;
            await _contentManager.PublishAsync(taxonomy);

            //force remove the new term content item to trigger events etc
            await _contentManager.RemoveAsync(taxonomyItem.ToObject<ContentItem>());

            await _notifier.SuccessAsync(H["Taxonomy item deleted successfully"]);

            return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = taxonomyContentItemId });
        }

        private JObject FindTaxonomyItem(JObject contentItem, string taxonomyItemId)
        {
            if (contentItem["ContentItemId"]?.Value<string>() == taxonomyItemId)
            {
                return contentItem;
            }

            if (contentItem.GetValue("Terms") == null)
            {
                return null;
            }

            var taxonomyItems = (JArray)contentItem["Terms"];

            JObject result;

            foreach (JObject taxonomyItem in taxonomyItems)
            {
                // Search in inner taxonomy items
                result = FindTaxonomyItem(taxonomyItem, taxonomyItemId);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private bool ValidateTaxonomyTerm(dynamic parent, ContentItem term)
        {
            JArray terms = _taxonomyHelper.GetTerms(JObject.FromObject(parent));
            return terms?.All(x => (string)x["ContentItemId"] == term.ContentItemId || (string)x["DisplayText"] != term.DisplayText) ?? true;
        }

        private IActionResult ValidationError(string errorMessage, dynamic model, string taxonomyContentItemId, string taxonomyItemId)
        {
            ModelState.AddModelError("", errorMessage);

            model.TaxonomyContentItemId = taxonomyContentItemId;
            model.TaxonomyItemId = taxonomyItemId;

            return View(model);
        }
    }
}
