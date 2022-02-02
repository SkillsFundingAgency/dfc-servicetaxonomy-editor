using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Indexes;
using DFC.ServiceTaxonomy.ContentApproval.Models.Enums;
using DFC.ServiceTaxonomy.ContentApproval.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.Services;
using YesSql;
using YesSql.Filters.Query;

namespace DFC.ServiceTaxonomy.ContentApproval.Services
{
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
                )
                .WithNamedTerm("reviewtype", builder => builder
                    .OneCondition((val, query, ctx) =>
                    {
                        if (Enum.TryParse<ReviewType>(val, true, out var reviewType))
                        {
                            query.With<ContentApprovalPartIndex>(i => i.ReviewType == (int)reviewType);
                        }
                        return new ValueTask<IQuery<ContentItem>>(query);
                    })
                    .MapTo<ContentApprovalContentsAdminListFilterViewModel>((val, model) =>
                    {
                        if (Enum.TryParse<ReviewType>(val, true, out var reviewType))
                        {
                            model.SelectedReviewType = reviewType;
                        }
                    })
                    .MapFrom<ContentApprovalContentsAdminListFilterViewModel>((model) =>
                    {
                        if(model.SelectedReviewType.HasValue)
                        {
                            return (true, model.SelectedReviewType.ToString());
                        }
                        return (false, String.Empty);
                    })
                );
        }
    }
}
