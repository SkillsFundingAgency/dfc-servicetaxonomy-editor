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
            return Shape("ContentsVisualiseAction_SummaryAdmin", new ContentVisualiseViewModel(model)).Location("SummaryAdmin", "ActionsMenu:20");
        }
    }
}
