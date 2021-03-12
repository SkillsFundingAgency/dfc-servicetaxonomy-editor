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

    public class ContentApprovalItemStatusDashboardPart : ContentPart
    {
        [DefaultValue(DashboardItemsStatusCard.InDraft)]
        public DashboardItemsStatusCard Card { get; set; } = DashboardItemsStatusCard.InDraft;
    }
}
