using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Models
{
    public class ContentVisualiseViewModel : ShapeViewModel
    {
        public ContentVisualiseViewModel(ContentItem contentItem, bool editMode = false)
        {
            ContentItem = contentItem;
            EditMode = editMode;
        }

        public ContentItem ContentItem { get; set; }
        public bool EditMode { get; set; }
    }
}
