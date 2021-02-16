using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    public class ContentApprovalPart : ContentPart
    {
        public ContentApprovalStatus ApprovalStatus { get; set; }
        public string? Comment { get; set; }
    }
}
