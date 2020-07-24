using DFC.ServiceTaxonomy.GraphVisualiser.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Drivers
{
    public class ContentVisualiseDriver : ContentDisplayDriver
    {
        public override IDisplayResult Display(ContentItem model, IUpdateModel updater)
        {
            return Shape("ContentVisualise_Button", new ContentVisualiseViewModel(model)).Location("SummaryAdmin", "Actions:10");
        }

        public override IDisplayResult Edit(ContentItem model, IUpdateModel updater)
        {
            return Shape("ContentVisualise_Button", new ContentVisualiseViewModel(model, true)).Location("Actions:after");
        }
    }
}
