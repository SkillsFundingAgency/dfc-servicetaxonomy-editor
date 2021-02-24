using DFC.ServiceTaxonomy.ContentApproval.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;

namespace DFC.ServiceTaxonomy.ContentApproval.Shapes
{
    public class ContentEditShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Content_Edit").OnDisplaying(context =>
            {
                dynamic shape = context.Shape;
                var approvalStatus = ((ContentItem)shape.ContentItem).As<ContentApprovalPart>()?.ReviewStatus;
                if (approvalStatus == ContentReviewStatus.InReview)
                {
                    Shape actions = (Shape)shape.Actions;
                    actions.Remove("Content_SaveDraftButton");
                }
            });
        }
    }
}
