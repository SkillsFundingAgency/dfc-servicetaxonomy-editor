using DFC.ServiceTaxonomy.ContentApproval.Models;

namespace DFC.ServiceTaxonomy.ContentApproval.Services
{
    public interface IContentItemsApprovalService
    {
        int ItemCount(DashboardItemsStatusCard itemStatus);
    }
}
