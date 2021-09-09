using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.CustomEditor.ViewModel
{
    public class CustomContentViewModel : ShapeViewModel
    {
        public CustomContentViewModel(ContentItem contentItem)
        {
            ContentItem = contentItem;
        }
        public ContentItem ContentItem { get; set; }
    }
}
