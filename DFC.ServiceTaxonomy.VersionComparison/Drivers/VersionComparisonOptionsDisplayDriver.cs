using System.Linq;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.VersionComparison.Drivers
{
    public class VersionComparisonOptionsDisplayDriver : DisplayDriver<VersionComparisonOptions>
    {
        public override IDisplayResult Edit(VersionComparisonOptions model)
        {
            return Combine(
                Initialize<VersionComparisonOptions>("VersionComparisonSelectLists", m => BuildUserOptionsViewModel(m, model)).Location("Search:10")
                );
        }

        private static void BuildUserOptionsViewModel(VersionComparisonOptions m, VersionComparisonOptions model)
        {
            m.ContentItemId = model.ContentItemId;
            m.BaseVersion = model.BaseVersion;
            m.BaseVersionSelectListItems = model.BaseVersionSelectListItems;
            m.BaseVersionSelectListItems.First(s => s.Value == m.BaseVersion).Selected = true;
            m.CompareVersion = model.CompareVersion;
            m.CompareVersionSelectListItems = model.CompareVersionSelectListItems;
            m.CompareVersionSelectListItems.First(s => s.Value == m.CompareVersion).Selected = true;
        }
    }
}
