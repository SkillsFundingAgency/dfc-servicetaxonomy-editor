using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public class ValidationFailure
    {
        public ContentItem ContentItem { get; set; }
        public string Reason { get; set; }
        public string Endpoint { get; set; }

        public ValidationFailure(ContentItem contentItem, string reason, string endpoint)
        {
            ContentItem = contentItem;
            Reason = reason;
            Endpoint = endpoint;
        }
    }
}
