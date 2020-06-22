using System;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.UnpublishLater.Models
{
    public class UnpublishLaterPart : ContentPart
    {
        public DateTime? ScheduledUnpublishUtc { get; set; }
    }
}
