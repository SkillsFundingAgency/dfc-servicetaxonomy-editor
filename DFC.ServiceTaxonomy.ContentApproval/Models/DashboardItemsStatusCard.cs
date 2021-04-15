namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    public enum DashboardItemsStatusCard
    {
        InDraft,
        WaitingForReview,   // change to ReadyToReview to match ContentReviewStatus?
        InReview,
        Published
        // these will be on a report card and may or may not be part of this part (probably not)
        // EmergencyEdited,
        // Unpublished,
        // Deleted,
        // ForcePublished
    }
}
