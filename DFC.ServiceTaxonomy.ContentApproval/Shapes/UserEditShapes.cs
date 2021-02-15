using DFC.ServiceTaxonomy.ContentApproval.Models;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;

namespace DFC.ServiceTaxonomy.ContentApproval.Shapes
{
    public class UserEditShapes : IShapeTableProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserEditShapes(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Content_SummaryAdmin").OnDisplaying(context =>
            {
                var currentUser = _httpContextAccessor.HttpContext?.User;
                if (currentUser == null || !currentUser.IsInRole("Editor"))
                {
                    return;
                }
                dynamic shape = context.Shape;
                var approvalStatus = (((ContentItem)shape.ContentItem).As<ContentApprovalPart>()?.ApprovalStatus) ??
                                     ContentApprovalStatus.InDraft;
                if (approvalStatus == ContentApprovalStatus.InReview)
                {
                    Shape actions = (Shape)shape.Actions;
                    actions.Remove("ContentsButtonEdit_SummaryAdmin");
                }
            });
        }
    }
}
