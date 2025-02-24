using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Taxonomies.Models;
using DFC.ServiceTaxonomy.Taxonomies.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using YesSql;

namespace DFC.ServiceTaxonomy.Taxonomies.Controllers
{
    [Admin]
    public class TagController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentHandler> _contentHandlers;
        private readonly ISession _session;
        private readonly ILogger<TagController> _logger;

        public TagController(
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentHandler> contentHandlers,
            ISession session,
            ILogger<TagController> logger)
        {
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _contentDefinitionManager = contentDefinitionManager;
            _contentHandlers = contentHandlers;
            _session = session;
            _logger = logger;
        }

        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(string taxonomyContentItemId, string displayText)
        {
            _logger.LogInformation($"CreatePost taxonomyContentItemId {taxonomyContentItemId} displayText {displayText}");
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTaxonomies))
            {
                _logger.LogWarning($"CreatePost Unauthorized");
                return Unauthorized();
            }

            ContentItem taxonomy;

            var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync("Taxonomy");

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
                _logger.LogWarning($"CreatePost taxonomy {taxonomy}  notfound");
                return NotFound();
            }

            var part = taxonomy.As<TaxonomyPart>();

            // Create tag term but only run content handlers not content item display manager update editor.
            // This creates empty parts, if parts are attached to the tag term, with empty data.
            // But still generates valid autoroute paths from the handler. 
            var contentItem = await _contentManager.NewAsync(part.TermContentType);
            contentItem.DisplayText = displayText;
            contentItem.Weld<TermPart>();
            contentItem.Alter<TermPart>(t => t.TaxonomyContentItemId = taxonomyContentItemId);

            var updateContentContext = new UpdateContentContext(contentItem);

            await _contentHandlers.InvokeAsync((handler, updateContentContext) => handler.UpdatingAsync(updateContentContext), updateContentContext, _logger);
            await _contentHandlers.Reverse().InvokeAsync((handler, updateContentContext) => handler.UpdatedAsync(updateContentContext), updateContentContext, _logger);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"CreatePost ModelState {ModelState.IsValid} BadRequest");
                return BadRequest();
            }

            // Tag terms are always added to the root taxonomy element.
            taxonomy.Alter<TaxonomyPart>(part => part.Terms.Add(contentItem));

            // Auto publish draftable taxonomies when creating a new tag term.
            if (contentTypeDefinition.GetSettings<ContentTypeSettings>().Draftable)
            {
                await _contentManager.PublishAsync(taxonomy);
            }
            else
            {
                await _session.SaveAsync(taxonomy);
            }

            var viewModel = new CreatedTagViewModel
            {
                ContentItemId = contentItem.ContentItemId,
                DisplayText = contentItem.DisplayText
            };

            return Ok(viewModel);
        }
    }
}

