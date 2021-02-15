using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    public enum DashboardItemsStatusCard
    {
        InDraft,
        WaitingForReview,
        InReview,
        Published
        // these will be on a report card and may or may not be part of this part (probably not)
        // ForcePublished,
        // EmergencyEdited,
        // Unpublished,
        // Deleted
    }

    public enum ItemsStatus
    {
        InDraft,
        WaitingForReview_UX,
        WaitingForReview_SME,
        WaitingForReview_Stakeholder,
        WaitingForReview_ContentDesign,
        InReview_UX,
        InReview_SME,
        InReview_Stakeholder,
        InReview_ContentDesign,
        Published
    }

    public class ContentApprovalItemStatusDashboardPart : ContentPart
    {
        [DefaultValue(DashboardItemsStatusCard.InDraft)]
        public DashboardItemsStatusCard Card { get; set; } = DashboardItemsStatusCard.InDraft;
    }
}
