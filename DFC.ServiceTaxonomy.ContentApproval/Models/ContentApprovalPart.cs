using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    public class ContentApprovalPart : ContentPart
    {
        public ContentReviewStatus ReviewStatus { get; set; }
        public ReviewType ReviewType { get; set; }
        public bool IsForcePublished { get; set; }
        public string? Comment { get; set; }
    }
}
