using DFC.ServiceTaxonomy.GraphSync.Activities.Events;
using DFC.ServiceTaxonomy.GraphSync.ViewModels;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Display;

namespace DFC.ServiceTaxonomy.GraphSync.Drivers.Events
{
    public class ContentTypeDeletedEventDisplay : ActivityDisplayDriver<ContentTypeDeletedEvent, ContentTypeDeletedEventViewModel>
    {
        public ContentTypeDeletedEventDisplay()
        {
        }

        public override IDisplayResult Display(ContentTypeDeletedEvent model)
        {
            return Combine(
                 Shape("ContentTypeDeletedEventDisplay_Fields_Thumbnail", new ContentTypeDeletedEventViewModel(model)).Location("Thumbnail", "Content"),
                 Factory("ContentTypeDeletedEventDisplay_Fields_Design", ctx =>
                 {
                     var shape = new ContentTypeDeletedEventViewModel();
                     shape.Activity = model;

                     return shape;
                 }).Location("Design", "Content")
             );
        }
    }
}
