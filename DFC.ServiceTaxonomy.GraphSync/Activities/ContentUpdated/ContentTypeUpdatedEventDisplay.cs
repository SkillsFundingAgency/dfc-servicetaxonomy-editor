using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Display;

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class ContentTypeUpdatedEventDisplay : ActivityDisplayDriver<ContentTypeUpdatedEvent, ContentTypeUpdatedViewModel>
    {
       public ContentTypeUpdatedEventDisplay()
        {
        }

        public override IDisplayResult Display(ContentTypeUpdatedEvent model)
        {
            return Combine(
                  Shape("ContentTypeUpdatedEventDisplay_Fields_Thumbnail", new ContentTypeUpdatedViewModel(model)).Location("Thumbnail", "Content"),
                  Factory("ContentTypeUpdatedEventDisplay_Fields_Design", ctx =>
                  {
                      var shape = new ContentTypeUpdatedViewModel();
                      shape.Activity = model;

                      return shape;
                  }).Location("Design", "Content")
              );
        }
    }
}
