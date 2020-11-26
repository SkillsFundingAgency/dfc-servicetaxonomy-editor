using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.ValidateAndRepair
{
    public class ValidationFailure
    {
        public ContentItem ContentItem { get; }
        public string Reason { get; }
        public ValidateType Type { get; }

        public ValidationFailure(ContentItem contentItem, string reason, ValidateType type = ValidateType.Merge)
        {
            ContentItem = contentItem;
            Reason = reason;
            Type = type;
        }
    }
}
