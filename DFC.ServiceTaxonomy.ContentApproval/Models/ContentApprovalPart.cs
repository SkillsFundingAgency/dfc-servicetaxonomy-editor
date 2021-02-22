using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    public class ContentApprovalPart : ContentPart
    {
        public ContentReviewStatus? ReviewStatus { get; set; }
        public bool InDraft => this.ContentItem != null && this.ContentItem.Latest && !this.ContentItem.Published;
        public bool IsPublished => this.ContentItem != null && this.ContentItem.Latest && this.ContentItem.Published;
        public string? Comment { get; set; }
    }
}
