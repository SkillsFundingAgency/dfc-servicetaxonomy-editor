using DFC.ServiceTaxonomy.ContentApproval.Models;

namespace DFC.ServiceTaxonomy.ContentApproval.ViewModels
{
    public class ContentApprovalItemStatusDashboardPartViewModel
    {
        public DashboardItemsStatusCard Card { get; set; }

        public ContentItemsApprovalCounts? ContentItemsApprovalCounts { get; set; }
    }
}
