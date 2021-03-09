using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Models;

namespace DFC.ServiceTaxonomy.ContentApproval.Services
{
    public interface IContentItemsApprovalService
    {
        Task<ContentItemsApprovalCounts> GetManageContentItemCount(DashboardItemsStatusCard card);
    }
}
