using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Models
{
    public class ContentVisualiseViewModel : ShapeViewModel
    {
        public ContentVisualiseViewModel(ContentItem contentItem, string? buttonSize = null)
        {
            ContentItem = contentItem;
            ButtonSize = buttonSize;
        }

        public ContentItem ContentItem { get; set; }
        public string? ButtonSize { get; set; }
    }
}
