using System.ComponentModel.DataAnnotations;

namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    public enum ContentReviewStatus
    {
        // the display names are currently only used by the contents item filter,
        // so the mismatch between NotInReview & "Will need review" is ok for now.
        // if we need to use the display name "Not In Review" we could have separate attributes
        [Display(Name="Will need review", Order = 0)]
        NotInReview,
        [Display(Name="Ready for review", Order = 1)]
        ReadyForReview,
        [Display(Name="In review", Order = 2)]
        InReview,
        [Display(Name = "Force published", Order = 3)]
        ForcePublished
    }
}
