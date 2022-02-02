using System.Collections.Generic;
using DFC.ServiceTaxonomy.ContentApproval.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.ContentManagement;
using YesSql.Filters.Query;

namespace DFC.ServiceTaxonomy.ContentApproval.ViewModels
{
    public class ReviewStatusContentsAdminListFilterViewModel
    {
        public ReviewStatusFilterOptions? SelectedApprovalStatus { get; set; }

        [BindNever]
        public List<SelectListItem>? ApprovalStatuses { get; set; }

        public QueryFilterResult<ContentItem>? FilterResult { get; set; }
    }
}
