using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Taxonomies.Helper;
using DFC.ServiceTaxonomy.Taxonomies.Validation;
using Newtonsoft.Json.Linq;
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

            JObject? parent = _taxonomyHelper.FindParentTaxonomyTerm(JObject.FromObject(term), JObject.FromObject(taxonomy));

            if (parent == null)
                throw new InvalidOperationException($"Could not find parent taxonomy term for {term}");

            if (term.DisplayText.Trim() != "/")
                return Task.FromResult(true);

            return Task.FromResult(parent.ToObject<ContentItem>()?.ContentType == Constants.TaxonomyContentType);
        }
    }
}
