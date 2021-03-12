using System.ComponentModel.DataAnnotations;

namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    // ContentItemsApprovalService assumes non-sparse enum values starting at 0!
    public enum ReviewType
    {
        [Display(Name="None", Order = 0)]
        None,
        [Display(Name="Content Design", Order = 1)]
        ContentDesign,
        [Display(Name="Stakeholder", Order = 2)]
        Stakeholder,
        [Display(Name="SME", Order = 3)]
        SME,
        [Display(Name="UX", Order = 4)]
        UX
    }
}
