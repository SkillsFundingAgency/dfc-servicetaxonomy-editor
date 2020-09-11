using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.ValidateAndRepair
{
    public class ValidationFailure
    {
        public ContentItem ContentItem { get; }
        public string Reason { get; }
        public FailureType Type { get; }

        public ValidationFailure(ContentItem contentItem, string reason, FailureType type = FailureType.Merge)
        {
            ContentItem = contentItem;
            Reason = reason;
            Type = type;
        }
    }

    public enum FailureType
    {
        Merge,
        Delete
    }
}
