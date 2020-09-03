using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Content;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using DFC.ServiceTaxonomy.Taxonomies.Helper;
using DFC.ServiceTaxonomy.Taxonomies.Models;
using DFC.ServiceTaxonomy.Taxonomies.Validation;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.PageLocation.Validators
{
    public class PageLocationsTaxonomyValidator : ITaxonomyValidator
    {
        private readonly IContentItemsService _contentItemsService;
        private readonly ITaxonomyHelper _taxonomyHelper;
        private IEnumerable<ITaxonomyTermValidator> _validators;

        public PageLocationsTaxonomyValidator(IContentItemsService contentItemsService, ITaxonomyHelper taxonomyHelper, IEnumerable<ITaxonomyTermValidator> validators)
        {
            _contentItemsService = contentItemsService;
            _taxonomyHelper = taxonomyHelper;
            _validators = validators;
        }

        public async Task<TaxonomyValidationResult> Validate(TaxonomyPart part)
        {
            if (part.TermContentType != ContentTypes.PageLocation)
            {
                return new TaxonomyValidationResult(true, null);
            }

            List<string> errors = new List<string>();

            //make sure nothing has moved that has associated pages anywhere down the tree
            List<ContentItem> allPages = await _contentItemsService.GetActive(ContentTypes.Page);

            JArray? terms = _taxonomyHelper.GetAllTerms(JObject.FromObject(part.ContentItem));

            if (terms != null)
            {
                foreach (JObject term in terms)
                {
                    dynamic? originalParent = _taxonomyHelper.FindParentTaxonomyTerm(term, JObject.FromObject(part.ContentItem));
                    dynamic? newParent = _taxonomyHelper.FindParentTaxonomyTerm(term, JObject.FromObject(part));

                    if (originalParent == null || newParent == null)
                        throw new InvalidOperationException($"Could not find {(originalParent == null ? "original" : "new")} parent taxonomy term for {term}");

                    if (newParent?.ContentItemId != null && newParent?.ContentItemId != originalParent?.ContentItemId)
                    {
                        //find all child terms down the taxonomy tree
                        var childTermsFromTree = _taxonomyHelper.GetAllTerms(term);

                        if (allPages.Any(x => (string)x.Content.Page.PageLocations.TermContentItemIds[0] == (string)term["ContentItemId"]! || childTermsFromTree.Any(t => (string)t["ContentItemId"]! == (string)x.Content.Page.PageLocations.TermContentItemIds[0])))
                        {
                            errors.Add("You cannot move a Page Location which has associated Pages linked to it, or any of its children.");
                        }

                        foreach (var validator in _validators)
                        {
                            if (!await validator.Validate(term, JObject.FromObject(part)))
                            {
                                errors.Add(validator.ErrorMessage);
                            }
                        }

                        //make sure display text doesn't clash with any other term at this level
                        JArray parentTerms = _taxonomyHelper.GetTerms(JObject.FromObject(newParent));
                        if (parentTerms?.Any(x => (string)x["ContentItemId"]! != (string)term["ContentItemId"]! && (string)x["DisplayText"]! == (string)term["DisplayText"]!) ?? false)
                        {
                            errors.Add("Terms at the same hierarchical position must have unique titles.");
                        }
                    }
                }
            }

            return new TaxonomyValidationResult(!errors.Any(), errors);
        }
    }
}
