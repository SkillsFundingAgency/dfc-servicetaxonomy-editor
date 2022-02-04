using System.Collections.Generic;
using DFC.ServiceTaxonomy.ContentApproval.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DFC.ServiceTaxonomy.ContentApproval.ViewModels
{
    public class ContentApprovalContentsAdminListFilterViewModel
    {
        // Content approval review status filtering
        public ReviewStatusFilterOptions? SelectedReviewStatus { get; set; }

        [BindNever]
        public List<SelectListItem>? ReviewStatuses { get; set; }

        // Content approval review types filtering
        public ReviewType? SelectedReviewType { get; set; }

        [BindNever]
        public List<SelectListItem>? ReviewTypes { get; set; }
    }
}
