using DFC.ServiceTaxonomy.VersionComparison.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.VersionComparison.Drivers
{
    public class DiffItemDisplayDriver : DisplayDriver<DiffItem>
    {
        public override IDisplayResult Display(DiffItem model)
        {
            return Combine(
                Initialize<DiffItem>("DiffItemDetailAdmin", diffItem => BuildViewModel(model, diffItem))
                    .Location("SummaryAdmin", "DiffItem")
                );
        }

        private static void BuildViewModel(DiffItem model, DiffItem diffItem)
        {
            diffItem.BaseItem = model.BaseItem;
            diffItem.CompareItem = model.CompareItem;
            diffItem.Name = model.Name;
        }
    }
}
