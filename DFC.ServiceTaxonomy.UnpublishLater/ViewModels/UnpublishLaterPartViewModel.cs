using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.UnpublishLater.ViewModels
{
    public class UnpublishLaterPartViewModel
    {
        [BindNever]
        public ContentItem? ContentItem { get; set; }
        public DateTime? ScheduledUnpublishUtc { get; set; }
        public DateTime? ScheduledUnpublishLocalDateTime { get; set; }
    }
}
