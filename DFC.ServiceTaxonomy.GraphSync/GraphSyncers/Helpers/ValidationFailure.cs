using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public class ValidationFailure
    {
        public ContentItem ContentItem { get; set; }
        public string Reason { get; set; }

        public ValidationFailure(ContentItem contentItem, string reason)
        {
            ContentItem = contentItem;
            Reason = reason;
        }
    }
}
