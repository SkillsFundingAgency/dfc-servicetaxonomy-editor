using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Taxonomies.Helper;
using DFC.ServiceTaxonomy.Taxonomies.Validation;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.PageLocation.Validators
{
    public class PageLocationTitleValidator : ITaxonomyTermValidator
    {
        private readonly ITaxonomyHelper _taxonomyHelper;

        public PageLocationTitleValidator(ITaxonomyHelper taxonomyHelper)
        {
            _taxonomyHelper = taxonomyHelper;
        }

        public string ErrorMessage => "'/' is not a valid Title for this page location";

        public Task<bool> Validate(ContentItem term, ContentItem taxonomy)
        {
            if (term.Content.PageLocation == null)
            {
                return Task.FromResult(true);
            }

            var parent = _taxonomyHelper.FindParentTaxonomyTerm(term, taxonomy);

            //only the very first page location can equal "/"
            return Task.FromResult(term.DisplayText.Trim() != "/" || parent?.ContentType == Constants.TaxonomyContentType ?? true);
        }
    }
}
