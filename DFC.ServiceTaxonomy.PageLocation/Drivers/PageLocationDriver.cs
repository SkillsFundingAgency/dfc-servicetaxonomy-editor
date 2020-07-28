using DFC.ServiceTaxonomy.PageLocation.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.PageLocation.Drivers
{
    public class PageLocationDriver : ContentDisplayDriver
    {
        public override IDisplayResult Display(ContentItem model, IUpdateModel updater)
        {
            return Shape("PageLocation_Tag", new PageLocationViewModel(model)).Location("SummaryAdmin", "Tags: 20");
        }
    }
}
