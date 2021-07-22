using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.Models.Enums;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.AuditTrail.Models;

namespace DFC.ServiceTaxonomy.ContentApproval.Handlers
{
    public class ContentApprovalAuditTrailEventHandler : AuditTrailEventHandlerBase
    {
        public override Task CreateAsync(AuditTrailCreateContext context)
        {
            if (context is AuditTrailCreateContext<AuditTrailContentEvent> contentEvent)
            {
                var contentApprovalPart = contentEvent.AuditTrailEventItem.ContentItem.As<ContentApprovalPart>();
                if (contentApprovalPart == null)
                {
                    return Task.CompletedTask;
                }

                if (contentApprovalPart.ReviewStatus == ReviewStatus.InReview)
                {
                    contentEvent.Name = "In review";
                }
                if (contentApprovalPart.ReviewStatus == ReviewStatus.ReadyForReview)
                {
                    contentEvent.Name = "Submitted for review";
                }

                if (context.Name == "Published" && contentApprovalPart.IsForcePublished)
                {
                    contentEvent.Name = "Force published";
                }
            }

            return Task.CompletedTask;
        }
    }
}
