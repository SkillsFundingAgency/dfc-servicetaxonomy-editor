using System.Linq;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;

namespace DFC.ServiceTaxonomy.ContentApproval.Shapes
{
    public class ContentEditShapes : IShapeTableProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContentEditShapes(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Content_Edit").OnDisplaying(context =>
            {
                dynamic shape = context.Shape;
                Shape actions = (Shape)shape.Actions;
                // Hide draft button is item is in review
                var approvalStatus = ((ContentItem)shape.ContentItem).As<ContentApprovalPart>()?.ReviewStatus ?? ContentReviewStatus.NotInReview;
                if (approvalStatus == ContentReviewStatus.InReview)
                {
                    actions.Remove("Content_SaveDraftButton");
                }
                // Use must have review permissions to be able to publish
                // TODO: ideally would use the authorize service here however there are issues with dependent objects already having been disposed of
                var currentUser = _httpContextAccessor.HttpContext?.User;
                if (currentUser == null || !currentUser.HasClaim(c =>
                    Permissions.CanPerformReviewPermissions.CanPerformReviewPermission.ImpliedBy.Any(p =>
                        p.Name == c.Value)))
                {
                    actions.Remove("Content_PublishButton");
                }
            });
        }
    }
}
