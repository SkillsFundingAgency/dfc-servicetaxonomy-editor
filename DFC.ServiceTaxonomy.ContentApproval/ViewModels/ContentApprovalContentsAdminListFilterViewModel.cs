using System.Collections.Generic;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DFC.ServiceTaxonomy.ContentApproval.ViewModels
{
    public class ContentApprovalContentsAdminListFilterViewModel
    {
        public ContentReviewStatus? SelectedApprovalStatus { get; set; }

        [BindNever]
        public List<SelectListItem>? ApprovalStatuses { get; set; }
    }
}
