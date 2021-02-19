using DFC.ServiceTaxonomy.ContentApproval.Models;

namespace DFC.ServiceTaxonomy.ContentApproval.ViewModels
{
    public class ContentApprovalPartViewModel
    {
        public ContentReviewStatus? ReviewStatus { get; set; }
        public string? Comment { get; set; }
        public string? ContentItemId { get; set; }
    }
}
