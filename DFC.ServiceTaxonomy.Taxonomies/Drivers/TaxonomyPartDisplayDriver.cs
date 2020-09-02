using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Taxonomies.Helper;
using DFC.ServiceTaxonomy.Taxonomies.Models;
using DFC.ServiceTaxonomy.Taxonomies.Validation;
using DFC.ServiceTaxonomy.Taxonomies.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using YesSql;

namespace DFC.ServiceTaxonomy.Taxonomies.Drivers
{
    public class TaxonomyPartDisplayDriver : ContentPartDisplayDriver<TaxonomyPart>
    {
        private readonly ITaxonomyHelper _taxonomyHelper;
        private readonly ISession _session;
        private readonly IEnumerable<ITaxonomyTermValidator> _validators;

        public TaxonomyPartDisplayDriver(ITaxonomyHelper taxonomyHelper, ISession session, IEnumerable<ITaxonomyTermValidator> validators)
        {
            _taxonomyHelper = taxonomyHelper;
            _session = session;
            _validators = validators;
        }

        public override IDisplayResult Display(TaxonomyPart part, BuildPartDisplayContext context)
        {
            var hasItems = part.Terms.Any();
            return Initialize<TaxonomyPartViewModel>(hasItems ? "TaxonomyPart" : "TaxonomyPart_Empty", m =>
            {
                m.ContentItem = part.ContentItem;
                m.TaxonomyPart = part;
            })
            .Location("Detail", "Content:5");
        }

        public override IDisplayResult Edit(TaxonomyPart part)
        {
            return Initialize<TaxonomyPartEditViewModel>("TaxonomyPart_Edit", model =>
            {
                model.TermContentType = part.TermContentType;
                model.TaxonomyPart = part;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(TaxonomyPart part, IUpdateModel updater)
        {
            var model = new TaxonomyPartEditViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix, t => t.Hierarchy, t => t.TermContentType))
            {
                if (!String.IsNullOrWhiteSpace(model.Hierarchy))
                {
                    var originalTaxonomyItems = part.ContentItem.As<TaxonomyPart>();

                    var newHierarchy = JArray.Parse(model.Hierarchy);      

                    var taxonomyItems = new JArray();

                    foreach (var item in newHierarchy)
                    {
                        taxonomyItems.Add(ProcessItem(originalTaxonomyItems, item as JObject));
                    }                    

                    part.Terms = taxonomyItems.ToObject<List<ContentItem>>();

                    //TODO : ultimately this logic needs to move to keep taxonomies generic, not sure how it ended up here!
                    //make sure nothing has moved that has associated pages anywhere down the tree
                    //TODO : I think we need to change how this queries for pages and check both pub and draft separately as we do elsewhere?
                    IEnumerable<ContentItem> allPages = await _session
                        .Query<ContentItem, ContentItemIndex>(x => x.ContentType == "Page" && x.Latest)
                        .ListAsync();

                    var terms = _taxonomyHelper.GetAllTerms(JObject.FromObject(part.ContentItem));

                    foreach (JObject term in terms)
                    {
                        dynamic originalParent = _taxonomyHelper.FindParentTaxonomyTerm(term, JObject.FromObject(part.ContentItem));
                        dynamic newParent = _taxonomyHelper.FindParentTaxonomyTerm(term, JObject.FromObject(part));

                        if (originalParent == null || newParent == null)
                            throw new InvalidOperationException($"Could not find parent taxonomy term for {(originalParent == null ? originalParent : newParent)}");

                        if (newParent.ContentItemId != null && newParent.ContentItemId != originalParent.ContentItemId)
                        {
                            //find all child terms down the taxonomy tree
                            var childTermsFromTree = _taxonomyHelper.GetAllTerms(term);
                            
                            if (allPages.Any(x => childTermsFromTree.Any(t => (string)t["ContentItemId"] == (string)x.Content.Page.PageLocations.TermContentItemIds[0])))
                            {
                                updater.ModelState.AddModelError(Prefix, nameof(TaxonomyPart.Terms), "You cannot move a Page Location which has associated Pages linked to it, or any of its children.");
                            }

                            foreach (var validator in _validators)
                            {
                                if (!await validator.Validate(term, JObject.FromObject(part)))
                                {
                                    updater.ModelState.AddModelError(Prefix, nameof(TaxonomyPart.Terms), validator.ErrorMessage);
                                }
                            }

                            //make sure display text doesn't clash with any other term at this level
                            JArray parentTerms = _taxonomyHelper.GetTerms(JObject.FromObject(newParent));
                            if (parentTerms?.Any(x => (string)x["ContentItemId"] != (string)term["ContentItemId"] && (string)x["DisplayText"] == (string)term["DisplayText"]) ?? false)
                            {
                                updater.ModelState.AddModelError(Prefix, nameof(TaxonomyPart.Terms), "Terms at the same hierarchical position must have unique titles.");
                            }
                        }
                    }
                }

                part.TermContentType = model.TermContentType;
            }

            return Edit(part);
        }

        /// <summary>
        /// Clone the content items at the specific index.
        /// </summary>
        private JObject GetTaxonomyItemAt(List<ContentItem> taxonomyItems, int[] indexes)
        {
            ContentItem taxonomyItem = null;

            // Seek the term represented by the list of indexes
            foreach (var index in indexes)
            {
                if (taxonomyItems == null || taxonomyItems.Count < index)
                {
                    // Trying to acces an unknown index
                    return null;
                }

                taxonomyItem = taxonomyItems[index];

                var terms = taxonomyItem.Content.Terms as JArray;
                taxonomyItems = terms?.ToObject<List<ContentItem>>();
            }

            var newObj = JObject.Parse(JsonConvert.SerializeObject(taxonomyItem));

            if (newObj["Terms"] != null)
            {
                newObj["Terms"] = new JArray();
            }

            return newObj;
        }

        private JObject ProcessItem(TaxonomyPart originalItems, JObject item)
        {
            var contentItem = GetTaxonomyItemAt(originalItems.Terms, item["index"].ToString().Split('-').Select(x => Convert.ToInt32(x)).ToArray());

            var children = item["children"] as JArray;

            if (children != null)
            {
                var taxonomyItems = new JArray();

                for (var i = 0; i < children.Count; i++)
                {
                    taxonomyItems.Add(ProcessItem(originalItems, children[i] as JObject));
                    contentItem["Terms"] = taxonomyItems;
                }
            }

            return contentItem;
        }
    }
}
