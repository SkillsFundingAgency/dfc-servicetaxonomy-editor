using DFC.ServiceTaxonomy.GraphSync.Activities.Events;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Display;

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class ContentTypeFieldRemovedEventDisplay : ActivityDisplayDriver<ContentTypeFieldRemovedEvent, ContentTypeFieldRemovedEventViewModel>
    {
       public ContentTypeFieldRemovedEventDisplay()
        {
        }

        public override IDisplayResult Display(ContentTypeFieldRemovedEvent model)
        {
            return Combine(
                  Shape("ContentTypeFieldRemovedEventDisplay_Fields_Thumbnail", new ContentTypeFieldRemovedEventViewModel(model)).Location("Thumbnail", "Content"),
                  Factory("ContentTypeFieldRemovedEventDisplay_Fields_Design", ctx =>
                  {
                      var shape = new ContentTypeFieldRemovedEventViewModel();
                      shape.Activity = model;

                      return shape;
                  }).Location("Design", "Content")
              );
        }
    }
}
