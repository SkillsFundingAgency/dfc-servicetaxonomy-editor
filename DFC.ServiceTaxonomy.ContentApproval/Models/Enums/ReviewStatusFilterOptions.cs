using System.ComponentModel.DataAnnotations;

namespace DFC.ServiceTaxonomy.ContentApproval.Models.Enums
{
    // Separate enum list for the filter as Force published, whilst related, is not strictly a review status.
    // Also allows for NotInReview to be be renamed to WillNeedReview for more readable filter purposes.
    public enum ReviewStatusFilterOptions
    {
        [Display(Name = "Will need review", Order = 0)]
        WillNeedReview,

        [Display(Name = "Ready for review", Order = 1)]
        ReadyForReview,

        [Display(Name = "In review", Order = 2)]
        InReview,

        [Display(Name = "Force published", Order = 3)]
        ForcePublished
    }
}
