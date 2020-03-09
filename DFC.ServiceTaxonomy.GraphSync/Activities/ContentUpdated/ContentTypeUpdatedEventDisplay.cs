using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Display;

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class ContentTypeUpdatedEventDisplay : ActivityDisplayDriver<ContentTypeUpdatedEvent, ContentTypeUpdatedViewModel>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private static string EditShapeType = $"{typeof(ContentTypeUpdatedEventDisplay).Name}_Fields_Edit";
        public ContentTypeUpdatedEventDisplay(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(ContentTypeUpdatedEvent model)
        {
            //return Initialize(EditShapeType, (System.Func<ContentTypeUpdatedViewModel, ValueTask>)(viewModel =>
            //{
            //    return EditActivityAsync(model, viewModel);
            //})).Location("Content");

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
