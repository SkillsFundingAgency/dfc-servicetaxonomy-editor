using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.ValidateAndRepair
{
    public class RepairFailure
    {
        public ContentItem ContentItem { get; set; }
        public string Reason { get; set; }

        public RepairFailure(ContentItem contentItem, string reason)
        {
            ContentItem = contentItem;
            Reason = reason;
        }
    }
}
