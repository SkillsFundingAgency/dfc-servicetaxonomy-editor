using System.ComponentModel.DataAnnotations;

namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    public enum ContentReviewStatus
    {
        [Display(Name="Not in review")]
        NotInReview,
        [Display(Name="Ready for review")]
        ReadyForReview,
        [Display(Name="In review")]
        InReview,
        [Display(Name="Force published")]
        ForcePublished
    }
}
