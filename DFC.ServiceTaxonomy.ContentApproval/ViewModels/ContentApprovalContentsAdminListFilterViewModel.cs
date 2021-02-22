using DFC.ServiceTaxonomy.ContentApproval.Models;

namespace DFC.ServiceTaxonomy.ContentApproval.ViewModels
{
    public class ContentApprovalContentsAdminListFilterViewModel
    {
        public ContentReviewStatus? SelectedApprovalStatus { get; set; }

        // [BindNever]
        // public List<SelectListItem> ApprovalStatuses { get; set; }
    }
}
