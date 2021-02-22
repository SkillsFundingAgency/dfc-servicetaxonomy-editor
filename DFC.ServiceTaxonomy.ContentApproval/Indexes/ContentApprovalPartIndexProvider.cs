﻿using DFC.ServiceTaxonomy.ContentApproval.Models;
using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.ContentApproval.Indexes
{
    public class ContentApprovalPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<ContentApprovalPartIndex>()
                .Map(contentItem =>
                {
                    ContentApprovalPart? contentApprovalPart = contentItem.As<ContentApprovalPart?>();
                    if (contentApprovalPart == null)
                    {
#pragma warning disable CS8603 // Possible null reference return.
                        return null;
#pragma warning restore CS8603 // Possible null reference return.
                    }

                    return new ContentApprovalPartIndex
                    {
                        ReviewStatus = contentApprovalPart.ReviewStatus
                    };
                });
        }
    }
}
