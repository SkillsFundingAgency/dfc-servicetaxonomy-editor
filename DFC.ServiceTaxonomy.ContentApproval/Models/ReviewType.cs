using System.ComponentModel.DataAnnotations;

namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    // ContentItemsApprovalService assumes non-sparse enum values starting at 0!
    public enum ReviewType
    {
        [Display(Name="None")]
        None,
        [Display(Name="Content Design")]
        ContentDesign,
        [Display(Name="Stakeholder")]
        Stakeholder,
        [Display(Name="SME")]
        SME,
        [Display(Name="UX")]
        UX
    }
}
