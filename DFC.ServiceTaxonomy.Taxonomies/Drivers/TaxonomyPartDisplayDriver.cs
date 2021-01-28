using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Taxonomies.Helper;
using DFC.ServiceTaxonomy.Taxonomies.Models;
using DFC.ServiceTaxonomy.Taxonomies.Validation;
using DFC.ServiceTaxonomy.Taxonomies.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace DFC.ServiceTaxonomy.Taxonomies.Drivers
{
    public class TaxonomyPartDisplayDriver : ContentPartDisplayDriver<TaxonomyPart>
    {
        private readonly ITaxonomyHelper _taxonomyHelper;
        private readonly IEnumerable<ITaxonomyValidator> _validators;

        public TaxonomyPartDisplayDriver(ITaxonomyHelper taxonomyHelper, IEnumerable<ITaxonomyValidator> validators)
        {
            _taxonomyHelper = taxonomyHelper;
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

                    foreach (var validator in _validators)
                    {
                        TaxonomyValidationResult result = await validator.Validate(part);
                        if (!result.Valid)
                        {
                            foreach (var error in result.Errors)
                            {
                                updater.ModelState.AddModelError(Prefix, nameof(TaxonomyPart.Terms), error);
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
