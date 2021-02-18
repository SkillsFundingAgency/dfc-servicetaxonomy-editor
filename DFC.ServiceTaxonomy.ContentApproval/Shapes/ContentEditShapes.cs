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
                var currentUser = _httpContextAccessor.HttpContext?.User;
                var reviewPermissions = Permissions.CanPerformReviewPermissions.CanPerformReviewPermission.ImpliedBy;
                if (currentUser == null || !currentUser.HasClaim(claim => reviewPermissions.Any(p => p.Name == claim.Value)))
                {
                    return;
                }

                dynamic shape = context.Shape;
                var approvalStatus = ((ContentItem)shape.ContentItem).As<ContentApprovalPart>()?.ApprovalStatus;
                if (approvalStatus != null)
                {
                    Shape actions = (Shape)shape.Actions;
                    actions.Remove("Content_PublishButton");
                    actions.Remove("Content_SaveDraftButton");
                }
            });
        }
    }
}
