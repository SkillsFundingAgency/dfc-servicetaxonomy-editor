using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    public enum DashboardItemsStatusCard
    {
        InDraft,
        // single WaitingForReview or one for each?
        // WaitingForReview_UX,
        // WaitingForReview_SME,
        // WaitingForReview_Stakeholder,
        // WaitingForReview_ContentDesign,
        WaitingForReview,
        // single InReview or one for each?
        // InReview_UX,
        // InReview_SME,
        // InReview_Stakeholder,
        // InReview_ContentDesign,
        InReview,
        Published
        // these will be on a report card and may or may not be part of this part (probably not)
        // ForcePublished,
        // EmergencyEdited,
        // Unpublished,
        // Deleted
    }

    public class ContentApprovalDashboardPart : ContentPart
    {
        [DefaultValue(DashboardItemsStatusCard.InDraft)]
        public DashboardItemsStatusCard Card { get; set; } = DashboardItemsStatusCard.InDraft;
    }
}
