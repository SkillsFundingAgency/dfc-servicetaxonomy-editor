using DFC.ServiceTaxonomy.ContentApproval.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;

namespace DFC.ServiceTaxonomy.ContentApproval.Shapes
{
    public class UserEditShapes : IShapeTableProvider
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserEditShapes(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Content_SummaryAdmin").OnDisplaying(context =>
            {
                var currentUser = _httpContextAccessor.HttpContext?.User;
                dynamic shape = context.Shape;
                if (currentUser == null || !(_authorizationService.AuthorizeAsync(currentUser, CommonPermissions.EditContent, shape)))
                {
                    return;
                }
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
