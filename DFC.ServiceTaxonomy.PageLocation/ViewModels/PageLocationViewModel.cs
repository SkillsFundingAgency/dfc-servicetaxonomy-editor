using System;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.PageLocation.ViewModels
{
    public class PageLocationViewModel : ShapeViewModel
    {
        public PageLocationViewModel(ContentItem contentItem)
        {
            IsDefault = contentItem.ContentType == "Page" &&
                Convert.ToBoolean(contentItem.Content.PageLocationPart.DefaultPageForLocation.Value);
        }

        public bool IsDefault { get; }
    }
}
