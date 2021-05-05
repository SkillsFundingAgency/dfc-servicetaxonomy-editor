using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    public class ContentApprovalItemStatusDashboardPart : ContentPart
    {
        [DefaultValue(DashboardItemsStatusCard.InDraft)]
        public DashboardItemsStatusCard Card { get; set; } = DashboardItemsStatusCard.InDraft;
    }
}
