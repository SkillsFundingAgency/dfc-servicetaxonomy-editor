using DFC.ServiceTaxonomy.ContentApproval.Models;

namespace DFC.ServiceTaxonomy.ContentApproval.ViewModels
{
    public class ContentApprovalPartViewModel
    {
        public ContentApprovalStatus? ApprovalStatus { get; set; }
        public string? Comment { get; set; }
        public string? ContentItemId { get; set; }
    }
}
