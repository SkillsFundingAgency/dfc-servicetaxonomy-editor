namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    public enum ContentApprovalStatus
    {
        InDraft,
        ReadyForReview_UX,
        ReadyForReview_SME,
        ReadyForReview_Stakeholder,
        ReadyForReview_ContentDesign,
        InReview_UX,
        InReview_SME,
        InReview_Stakeholder,
        InReview_ContentDesign,
        Published,
        ForcePublished
    }
}
