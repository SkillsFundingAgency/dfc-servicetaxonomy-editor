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

        public Task<bool> Validate(JObject term, JObject taxonomy)
        {
            ContentItem? termContentItem = term.ToObject<ContentItem>();

            if (termContentItem == null)
                throw new InvalidOperationException("You must provide a term");

            if (termContentItem.Content.PageLocation == null)
            {
                return Task.FromResult(true);
            }

            if (termContentItem.DisplayText.Trim() != "/")
                return Task.FromResult(true);

            JObject? parent = _taxonomyHelper.FindParentTaxonomyTerm(term, taxonomy);

            if (parent == null)
                throw new InvalidOperationException($"Could not find parent taxonomy term for {term}");

            return Task.FromResult(parent.ToObject<ContentItem>()?.ContentType == Constants.TaxonomyContentType);
        }
    }
}
