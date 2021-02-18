using System.ComponentModel.DataAnnotations;

namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    // public enum ContentApprovalStatus
    // {
    //     InDraft,
    //     ReadyForReview_UX,
    //     ReadyForReview_SME,
    //     ReadyForReview_Stakeholder,
    //     ReadyForReview_ContentDesign,
    //     InReview_UX,
    //     InReview_SME,
    //     InReview_Stakeholder,
    //     InReview_ContentDesign,
    //     Published,
    //     ForcePublished
    // }

    /*
     * should ContentApprovalStatus contain just ReadyForReview & InReview?
        otherwise, we'll need to ensure it's always set correctly for indraft/published and there may be too many integration points to make that work
        we can determine indraft and published the usual way - by checking the latest and published flags

        if we do keep indraft & published in here, will need a separate enum for the approval status filter
        might be better anyway, can then add All approval statuses??
     */

    public enum ContentApprovalStatus
    {
        [Display(Name="In Draft")]
        InDraft,
        [Display(Name="Ready For Review")]
        ReadyForReview,
        [Display(Name="In Review")]
        InReview,
        [Display(Name="Published")]
        Published
    }
}
