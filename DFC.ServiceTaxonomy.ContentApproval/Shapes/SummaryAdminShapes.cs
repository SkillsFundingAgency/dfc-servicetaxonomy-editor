﻿using DFC.ServiceTaxonomy.ContentApproval.Models;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;

namespace DFC.ServiceTaxonomy.ContentApproval.Shapes
{
    public class SummaryAdminShapes : IShapeTableProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SummaryAdminShapes(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Content_SummaryAdmin").OnDisplaying( context =>
            {
                var currentUser = _httpContextAccessor.HttpContext?.User;
                dynamic shape = context.Shape;
                if (currentUser == null || !currentUser.HasClaim(claim => claim.Value == "EditContent"))
                {
                    return;
                }
                var approvalStatus = ((ContentItem)shape.ContentItem).As<ContentApprovalPart>()?.ApprovalStatus;
                if (approvalStatus == ContentApprovalStatus.InReview)
                {
                    Shape actions = (Shape)shape.Actions;
                    actions.Remove("ContentsButtonEdit_SummaryAdmin");
                }
            });
        }
    }
}
