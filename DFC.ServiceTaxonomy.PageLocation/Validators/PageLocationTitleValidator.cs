using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.PageLocation.Constants;
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

        public Task<(bool, string)> ValidateCreate(JObject term, JObject taxonomy)
        {
            ContentItem? termContentItem = term.ToObject<ContentItem>();

            if (termContentItem == null)
                throw new InvalidOperationException("You must provide a term");

            if (termContentItem.Content.PageLocation == null)
            {
                return Task.FromResult((true, string.Empty));
            }

            if (termContentItem.DisplayText.Trim() != "/")
                return Task.FromResult((true, string.Empty));

            JObject? parent = _taxonomyHelper.FindParentTaxonomyTerm(term, taxonomy);

            if (parent == null)
                throw new InvalidOperationException($"Could not find parent taxonomy term for {term}");

            if (parent.ToObject<ContentItem>()?.ContentType == ContentTypes.Taxonomy)
                return Task.FromResult((true, string.Empty));

            return Task.FromResult((false, "'/' is not a valid Title for this page location"));
        }

        public Task<(bool, string)> ValidateUpdate(JObject term, JObject taxonomy)
        {
            return ValidateCreate(term, taxonomy);
        }

        public Task<(bool, string)> ValidateDelete(JObject term, JObject taxonomy)
        {
            return Task.FromResult((true, string.Empty));
        }
    }
}
