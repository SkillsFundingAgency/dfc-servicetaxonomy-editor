using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.ValidateAndRepair
{
    public class RepairFailure
    {
        public ContentItem ContentItem { get; set; }
        public string Reason { get; set; }
        public ValidateType Type { get; }

        public RepairFailure(ContentItem contentItem, string reason, ValidateType type = ValidateType.Merge)
        {
            ContentItem = contentItem;
            Reason = reason;
            Type = type;
        }
    }
}
