using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using DFC.ServiceTaxonomy.PageLocation.Constants;
using DFC.ServiceTaxonomy.Taxonomies.Helper;
using DFC.ServiceTaxonomy.Taxonomies.Validation;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.PageLocation.Validators
{
    public class PageLocationModificationValidator : ITaxonomyTermValidator
    {
        private readonly IContentItemsService _contentItemsService;
        private readonly ITaxonomyHelper _taxonomyHelper;

        public PageLocationModificationValidator(IContentItemsService contentItemsService, ITaxonomyHelper taxonomyHelper)
        {
            _contentItemsService = contentItemsService;
            _taxonomyHelper = taxonomyHelper;
        }

        public Task<(bool, string)> ValidateCreate(JsonObject term, JsonObject taxonomy)
        {
            return Task.FromResult((true, string.Empty));
        }

        public async Task<(bool, string)> ValidateUpdate(JsonObject term, JsonObject taxonomy)
        {
            if (!term.ContainsKey("PageLocation"))
            {
                return (true, string.Empty);
            }

            List<ContentItem> allPages = await _contentItemsService.GetActive(ContentTypes.Page);

            //find all child terms down the taxonomy tree
            JsonArray childTermsFromTree = _taxonomyHelper.GetAllTerms(term);

            if (allPages.Any(x => (string)x.Content.Page.PageLocations.TermContentItemIds[0] == (string)term["ContentItemId"]! || childTermsFromTree.Any(t => (string)t!["ContentItemId"]! == (string)x.Content.Page.PageLocations.TermContentItemIds[0])))
            {
                return (false, "Page Locations with pages associated to them or any of their children cannot be changed or deleted.");
            }

            return (true, string.Empty);
        }

        public Task<(bool, string)> ValidateDelete(JsonObject term, JsonObject taxonomy)
        {
            return ValidateUpdate(term, taxonomy);
        }
    }
}
