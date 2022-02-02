using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Indexes;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;
using YesSql.Filters.Query;

namespace DFC.ServiceTaxonomy.ContentApproval.Services
{
    public class ContentApprovalPartContentsAdminListFilter: IContentsAdminListFilter
    {
        public async Task FilterAsync(ContentOptionsViewModel model, IQuery<ContentItem> query, IUpdateModel updater)
        {
            var viewModel = new ContentApprovalContentsAdminListFilterViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Constants.ContentApprovalPartPrefix)
                && viewModel.SelectedReviewStatus.HasValue)
            {
                if (viewModel.SelectedReviewStatus == ReviewStatusFilterOptions.WillNeedReview)
                {
                    query.With<ContentItemIndex>(x => x.Latest && !x.Published);
                }

                if (viewModel.SelectedReviewStatus == ReviewStatusFilterOptions.ForcePublished)
                {
                    query.With<ContentApprovalPartIndex>(i => i.IsForcePublished);
                }
                else
                {
                    query.With<ContentApprovalPartIndex>(i => i.ReviewStatus == (int?)viewModel.SelectedReviewStatus);
                }
            }
        }
    }

    public class ContentApprovalPartContentsAdminListFilterProvider : IContentsAdminListFilterProvider
    {
        public void Build(QueryEngineBuilder<ContentItem> builder)
        {
            builder
                .WithNamedTerm("reviewstatus", builder => builder
                    .OneCondition((val, query, ctx) =>
                    {
                        if(Enum.TryParse<ReviewStatusFilterOptions>(val, true, out var reviewStatus))
                        {
                            switch(reviewStatus)
                            {
                                case ReviewStatusFilterOptions.WillNeedReview:
                                    query.With<ContentItemIndex>(x => x.Latest && !x.Published);
                                    break;
                                case ReviewStatusFilterOptions.ForcePublished:
                                    query.With<ContentApprovalPartIndex>(i => i.IsForcePublished);
                                    break;
                                default:
                                    query.With<ContentApprovalPartIndex>(i => i.ReviewStatus == (int)reviewStatus);
                                    break;
                            }
                        }

                        return new ValueTask<IQuery<ContentItem>>(query);
                    })
                    .MapTo<ContentApprovalContentsAdminListFilterViewModel>((val, model) =>
                    {
                        if (Enum.TryParse<ReviewStatusFilterOptions>(val, true, out var reviewStatus))
                        {
                            model.SelectedReviewStatus = reviewStatus;
                        }
                    })
                    .MapFrom<ContentApprovalContentsAdminListFilterViewModel>((model) =>
                    {
                        if (model.SelectedReviewStatus.HasValue)
                        {
                            return (true, model.SelectedReviewStatus.ToString());
                        }
                        return (false, String.Empty);
                    })
                );
        }
    }

    public class ContentApprovalContentsAdminListFilterViewModel
    {
        public ReviewStatusFilterOptions? SelectedReviewStatus { get; set; }

        [BindNever]
        public List<SelectListItem>? ReviewStatuses { get; set; }

    }
}
