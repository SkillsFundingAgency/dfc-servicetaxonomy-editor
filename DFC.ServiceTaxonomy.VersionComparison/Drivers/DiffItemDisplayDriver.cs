using System.Linq;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.VersionComparison.Drivers
{
    public class DiffItemDisplayDriver : DisplayDriver<DiffItem>
    {
        public override IDisplayResult Display(DiffItem model)
        {
            var linkShape = (model.BaseURLs?.Any() ?? false) || (model.CompareURLs?.Any() ?? false);
            var shapeView = $"DiffItemDetailAdmin{(linkShape ? "Link" : "Text")}";

            return Combine(
                Initialize<DiffItem>(shapeView, diffItem => BuildViewModel(model, diffItem))
                    .Location("SummaryAdmin", "DiffItem")
                );
        }

        private static void BuildViewModel(DiffItem model, DiffItem diffItem)
        {
            diffItem.BaseItem = model.BaseItem;
            diffItem.BaseURLs = model.BaseURLs;
            diffItem.CompareItem = model.CompareItem;
            diffItem.CompareURLs = model.CompareURLs;
            diffItem.Name = model.Name;
        }
    }
}
