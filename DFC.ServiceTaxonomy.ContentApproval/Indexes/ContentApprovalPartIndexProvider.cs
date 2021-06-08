using DFC.ServiceTaxonomy.ContentApproval.Models;
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
                        return new ContentApprovalPartIndex();
                    }

                    return new ContentApprovalPartIndex
                    {
                        ReviewStatus = (int)contentApprovalPart.ReviewStatus,
                        ReviewType = (int)contentApprovalPart.ReviewType,
                        IsForcePublished = contentApprovalPart.IsForcePublished
                    };
                });
        }
    }
}
