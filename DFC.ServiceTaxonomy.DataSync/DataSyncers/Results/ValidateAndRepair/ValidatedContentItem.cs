using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Results.ValidateAndRepair
{
    public class ValidatedContentItem
    {
        public ValidatedContentItem(ContentItem contentItem, ValidateType type = ValidateType.Merge)
        {
            ContentItem = contentItem;
            Type = type;
        }

        public ContentItem ContentItem { get; }
        public ValidateType Type { get; }
    }
}
