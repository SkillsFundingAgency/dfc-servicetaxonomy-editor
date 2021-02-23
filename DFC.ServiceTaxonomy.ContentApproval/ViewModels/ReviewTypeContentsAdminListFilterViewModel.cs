using System.Collections.Generic;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DFC.ServiceTaxonomy.ContentApproval.ViewModels
{
    public class ReviewTypeContentsAdminListFilterViewModel
    {
        public ReviewType? SelectedReviewType { get; set; }

        [BindNever]
        public List<SelectListItem>? ReviewTypes { get; set; }
    }
}
