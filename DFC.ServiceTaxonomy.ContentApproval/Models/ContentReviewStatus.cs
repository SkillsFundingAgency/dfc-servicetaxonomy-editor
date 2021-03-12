using System.ComponentModel.DataAnnotations;

namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    public enum ContentReviewStatus
    {
        // the display names are currently only used by the contents item filter,
        // so the mismatch between NotInReview & "Will need review" is ok for now.
        // if we need to use the display name "Not In Review" we could have separate attributes
        [Display(Name="Will need review")]
        NotInReview,
        [Display(Name="Ready for review")]
        ReadyForReview,
        [Display(Name="In review")]
        InReview,
        //[Display(Name = "Force published")]
        //ForcePublished
    }
}
