using DFC.ServiceTaxonomy.CustomEditor.ViewModel;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.CustomEditor.Drivers
{
    public class CustomContentDriver : ContentDisplayDriver
    {
        public override IDisplayResult Edit(ContentItem model, BuildEditorContext context)
        {
            return Shape("ContentEdit_AdditionalActions", new CustomContentViewModel(model)).Location("Actions:before");
        }
    }
}
