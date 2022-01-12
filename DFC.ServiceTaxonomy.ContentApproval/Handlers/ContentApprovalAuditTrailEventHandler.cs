using System;
using System.Linq;
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
            // Override event name if:
            // 1. it is audit trail content event
            // 2. it is a Save or Publish event only
            // 3. there is a content approval part add as part of the content type
            if (context is AuditTrailCreateContext<AuditTrailContentEvent> contentEvent &&
                new[] {Constants.ContentEvent_Saved, Constants.ContentEvent_Published}.Any(ev => ev.Equals(contentEvent.Name, StringComparison.CurrentCultureIgnoreCase)) &&
                contentEvent.AuditTrailEventItem.ContentItem.Has<ContentApprovalPart>())
            {
                var contentApprovalPart = contentEvent.AuditTrailEventItem.ContentItem.As<ContentApprovalPart>();
                if (contentApprovalPart == null)
                {
                    return Task.CompletedTask;
                }

                if (contentEvent.Name.Equals(Constants.ContentEvent_Published,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    ProcessPublishedEvent(contentEvent, contentApprovalPart);
                }
                else if (contentEvent.Name.Equals(Constants.ContentEvent_Saved,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    ProcessSavedEvent(contentEvent, contentApprovalPart);
                }
            }
            return Task.CompletedTask;
        }

        private static void ProcessPublishedEvent(AuditTrailCreateContext<AuditTrailContentEvent> publishedContentEvent, ContentApprovalPart contentApprovalPart)
        {
            if (contentApprovalPart.IsForcePublished)
            {
                switch (contentApprovalPart.ReviewType)
                {
                    case ReviewType.ContentDesign:
                        publishedContentEvent.Name = GetPublishedEventName(Constants.ReviewType_ContentDesign);
                        break;
                    case ReviewType.Stakeholder:
                        publishedContentEvent.Name = GetPublishedEventName(Constants.ReviewType_Stakeholder);
                        break;

                    case ReviewType.SME:
                        publishedContentEvent.Name = GetPublishedEventName(Constants.ReviewType_Sme);
                        break;

                    case ReviewType.UX:
                        publishedContentEvent.Name = GetPublishedEventName(Constants.ReviewType_Ux);
                        break;
                }
            }
        }

        private static void ProcessSavedEvent(AuditTrailCreateContext<AuditTrailContentEvent> savedContentEvent, ContentApprovalPart contentApprovalPart)
        {
            if (contentApprovalPart.ReviewStatus == ReviewStatus.RequiresRevision)
            {
                savedContentEvent.Name = Constants.ContentEventName_SendBack;
            }
            else
            {
                switch (contentApprovalPart.ReviewType)
                {
                    case ReviewType.ContentDesign:
                        savedContentEvent.Name = GetSavedEventName(contentApprovalPart, Constants.ReviewType_ContentDesign);
                        break;
                    case ReviewType.Stakeholder:
                        savedContentEvent.Name = GetSavedEventName(contentApprovalPart, Constants.ReviewType_Stakeholder);
                        break;
                    case ReviewType.SME:
                        savedContentEvent.Name = GetSavedEventName(contentApprovalPart, Constants.ReviewType_Sme);
                        break;
                    case ReviewType.UX:
                        savedContentEvent.Name = GetSavedEventName(contentApprovalPart, Constants.ReviewType_Ux);
                        break;
                }
            }
        }

        private static string GetPublishedEventName(string reviewType)
        {
            return $"Force published ({reviewType})";
        }

        private static string GetSavedEventName(ContentApprovalPart contentApprovalPart, string reviewType)
        {
            return contentApprovalPart.ReviewStatus != ReviewStatus.InReview
                ? $"Requested for review ({reviewType})"
                : $"In review ({reviewType})";
        }
    }
}
