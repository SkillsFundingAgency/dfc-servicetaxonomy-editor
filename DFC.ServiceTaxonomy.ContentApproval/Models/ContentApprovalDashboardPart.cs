using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    // public enum ContentApprovalDashboardView
    // {
    //     WaitingForReview_UX,
    //     WaitingForReview_Stakeholder
    // }

    public class ContentApprovalDashboardPart : ContentPart
    {
        //todo: use enum directly?
        public int ViewType { get; set; }
    }
}
