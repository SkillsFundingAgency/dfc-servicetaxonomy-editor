using System.ComponentModel.DataAnnotations;

namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
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
