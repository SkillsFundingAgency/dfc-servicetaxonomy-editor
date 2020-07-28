using System;
using DFC.ServiceTaxonomy.PageLocation.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.PageLocation.ViewModels
{
    public class PageLocationViewModel : ShapeViewModel
    {
        public PageLocationViewModel(ContentItem contentItem)
        {
            IsDefault = contentItem.ContentType == "Page" &&
                Convert.ToBoolean(contentItem.Content[nameof(PageLocationPart)][PageLocationPart.DefaultPageForLocationName].Value);
        }

        public bool IsDefault { get; }
    }
}
