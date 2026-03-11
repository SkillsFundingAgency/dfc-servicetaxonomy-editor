using DFC.ServiceTaxonomy.GraphVisualiser.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Drivers
{
    public class ContentVisualiseDriver : ContentDisplayDriver
    {
        public override IDisplayResult Display(ContentItem model, BuildDisplayContext context)
        {
            return Shape("ContentsVisualiseAction_SummaryAdmin", new ContentVisualiseViewModel(model)).Location("SummaryAdmin", "ActionsMenu:20");
        }
    }
}
