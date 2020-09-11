using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Content;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using DFC.ServiceTaxonomy.Taxonomies.Helper;
using DFC.ServiceTaxonomy.Taxonomies.Validation;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.PageLocation.Validators
{
    public class PageLocationDeleteValidator : ITaxonomyTermDeleteValidator
    {
        private readonly IContentItemsService _contentItemsService;
        private readonly ITaxonomyHelper _taxonomyHelper;

        public PageLocationDeleteValidator(IContentItemsService contentItemsService, ITaxonomyHelper taxonomyHelper)
        {
            _contentItemsService = contentItemsService;
            _taxonomyHelper = taxonomyHelper;
        }

        public string ErrorMessage => "Page Locations with pages associated to them or any of their children cannot be deleted.";

        public async Task<bool> Validate(JObject term, JObject taxonomy)
        {
            if (!term.ContainsKey("PageLocation"))
            {
                return true;
            }

            List<ContentItem> allPages = await _contentItemsService.GetActive(ContentTypes.Page);

            //find all child terms down the taxonomy tree
            var childTermsFromTree = _taxonomyHelper.GetAllTerms(term);

            if (allPages.Any(x => (string)x.Content.Page.PageLocations.TermContentItemIds[0] == (string)term["ContentItemId"]! || childTermsFromTree.Any(t => (string)t["ContentItemId"]! == (string)x.Content.Page.PageLocations.TermContentItemIds[0])))
            {
                return false;
            }

            return true;
        }
    }
}
