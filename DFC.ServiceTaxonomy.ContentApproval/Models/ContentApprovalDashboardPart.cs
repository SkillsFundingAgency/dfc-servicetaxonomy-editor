using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    public enum DashboardItemsStatusCard
    {
        InDraft,
        // single WaitingForReview or one for each?
        WaitingForReview_UX,
        WaitingForReview_SME,
        WaitingForReview_Stakeholder,
        WaitingForReview_ContentDesign,
        // single InReview or one for each?
        InReview_UX,
        InReview_SME,
        InReview_Stakeholder,
        InReview_ContentDesign,
        ForcePublished,
        EmergencyEdited,
        Unpublished,
        Deleted
    }

    public class ContentApprovalDashboardPart : ContentPart
    {
        [DefaultValue(DashboardItemsStatusCard.InDraft)]
        public DashboardItemsStatusCard Card { get; set; } = DashboardItemsStatusCard.InDraft;
    }
}
