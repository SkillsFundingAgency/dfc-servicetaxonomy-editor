using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.Models.Enums;
using DFC.ServiceTaxonomy.CustomEditor.Constants;
using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.AuditTrail.Models;

namespace DFC.ServiceTaxonomy.CustomEditor.Handlers.CustomAuditTrailEvents
{
    public class ContentApprovalAuditTrailEvent : ICustomAuditTrailEvent
    {
        public void HandleCustomEvent(AuditTrailCreateContext<AuditTrailContentEvent> contentEvent, HttpRequest request)
        {
            var contentApprovalPart = contentEvent.AuditTrailEventItem.ContentItem.As<ContentApprovalPart>();
            if (contentApprovalPart == null)
            {
                return;
            }

            if (contentEvent.Name.Equals(AuditTrail.ContentEvent_Published,
                StringComparison.InvariantCultureIgnoreCase))
            {
                ProcessPublishedEvent(contentEvent, contentApprovalPart);
            }
            else if (contentEvent.Name.Equals(AuditTrail.ContentEvent_Saved,
                StringComparison.InvariantCultureIgnoreCase))
            {
                ProcessSavedEvent(contentEvent, contentApprovalPart);
            }
        }

        private static void ProcessPublishedEvent(AuditTrailCreateContext<AuditTrailContentEvent> publishedContentEvent, ContentApprovalPart contentApprovalPart)
        {
            if (contentApprovalPart.IsForcePublished)
            {
                switch (contentApprovalPart.ReviewType)
                {
                    case ReviewType.ContentDesign:
                        publishedContentEvent.Name = GetPublishedEventName(AuditTrail.ReviewType_ContentDesign);
                        break;
                    case ReviewType.Stakeholder:
                        publishedContentEvent.Name = GetPublishedEventName(AuditTrail.ReviewType_Stakeholder);
                        break;

                    case ReviewType.SME:
                        publishedContentEvent.Name = GetPublishedEventName(AuditTrail.ReviewType_Sme);
                        break;

                    case ReviewType.UX:
                        publishedContentEvent.Name = GetPublishedEventName(AuditTrail.ReviewType_Ux);
                        break;
                }
            }
        }

        private static void ProcessSavedEvent(AuditTrailCreateContext<AuditTrailContentEvent> savedContentEvent, ContentApprovalPart contentApprovalPart)
        {
            if (contentApprovalPart.ReviewStatus == ReviewStatus.RequiresRevision)
            {
                savedContentEvent.Name = AuditTrail.ContentEventName_SendBack;
            }
            else
            {
                switch (contentApprovalPart.ReviewType)
                {
                    case ReviewType.ContentDesign:
                        savedContentEvent.Name = GetSavedEventName(contentApprovalPart, AuditTrail.ReviewType_ContentDesign);
                        break;
                    case ReviewType.Stakeholder:
                        savedContentEvent.Name = GetSavedEventName(contentApprovalPart, AuditTrail.ReviewType_Stakeholder);
                        break;
                    case ReviewType.SME:
                        savedContentEvent.Name = GetSavedEventName(contentApprovalPart, AuditTrail.ReviewType_Sme);
                        break;
                    case ReviewType.UX:
                        savedContentEvent.Name = GetSavedEventName(contentApprovalPart, AuditTrail.ReviewType_Ux);
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


        public bool ValidCustomEvent(AuditTrailCreateContext<AuditTrailContentEvent> contentEvent, HttpRequest request)
        {
            var contentApprovalPart = contentEvent.AuditTrailEventItem.ContentItem.As<ContentApprovalPart>();
            if (contentApprovalPart == null)
            {
                return false;
            }
            var requestForm = request.HasFormContentType ? request.Form : new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());

            if (contentEvent.Name.Equals(AuditTrail.ContentEvent_Saved, StringComparison.InvariantCultureIgnoreCase) &&
                (requestForm[AuditTrail.Save_Button_Key].ToString().StartsWith(AuditTrail.ContentApproval_Requires_Revision) ||
                requestForm[AuditTrail.Save_Button_Key].ToString().StartsWith(AuditTrail.ContentApproval_Request_Approval) ||
                contentApprovalPart.ReviewStatus == ReviewStatus.InReview))
            {
                return true;
            }
            else if (contentEvent.Name.Equals(AuditTrail.ContentEvent_Published, StringComparison.InvariantCultureIgnoreCase) &&
                contentApprovalPart.IsForcePublished)
            {
                return true;
            }

            return false;
        }
    }
}
