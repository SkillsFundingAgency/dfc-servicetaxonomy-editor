using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using OrchardCore.DisplayManagement.ModelBinding;

namespace DFC.ServiceTaxonomy.ContentApproval.Services
{
    public interface IContentItemsApprovalService
    {
        Task<ContentItemsApprovalCounts> GetManageContentItemCounts(DashboardItemsStatusCard card, IUpdateModel updater);
    }
}
