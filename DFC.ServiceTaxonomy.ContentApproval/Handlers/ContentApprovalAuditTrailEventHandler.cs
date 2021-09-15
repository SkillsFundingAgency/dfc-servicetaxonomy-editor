using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Models;
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
            /*
             *  Button/Action matrix         Key/Action                                                                   
             *  --------------------         ---------------------------------------------------------------------------  
             *  Publish                     - constants.SubmitPublishKey action = submit.Publish                          
             *  Publish and continue        - constants.SubmitPublishKey action = submit.PublishAndContinue               
             *  
             *  Force Publish
             *      Content Design          - constants.SubmitPublishKey action = submit.RequestApproval - ContentDesign  
             *      Stakeholder             - constants.SubmitPublishKey action = submit.RequestApproval - Stakeholder    
             *      SME                     - constants.SubmitPublishKey action = submit.RequestApproval - SME            
             *      UX                      - constants.SubmitPublishKey action = submit.RequestApproval - UX             
             *      
             *  Save Draft and continue     - constants.SubmitSaveKey    action = submit.SaveAndContinue                  
             *  Save Draft and exit         - constants.SubmitSaveKey    action = submit.Save                             
             *  
             *  Request review
             *      Content Design          - constants.SubmitSaveKey    action = submit.RequestApproval - ContentDesign  
             *      Stakeholder             - constants.SubmitSaveKey    action = submit.RequestApproval - Stakeholder    
             *      SME                     - constants.SubmitSaveKey    action = submit.RequestApproval - SME            
             *      UK                      - constants.SubmitSaveKey    action = submit.RequestApproval - UX             
             *      
             *  Send back                   - constants.SubmitSaveKey    action = submit.RequiresRevision                 
             *               
             *  Preview draft               - N/A (opens new tab)
             *  
             *  Visualise draft graph       - N/A (opens new tab)
             *  Visualise published graph   - N/A (opens new tab)
             *  
             *  Cancel                      - N/A (exits page)
             */

            if (context is AuditTrailCreateContext<AuditTrailContentEvent> contentEvent &&
                contentEvent.AuditTrailEventItem.ContentItem.Has<ContentApprovalPart>())
            {
                var contentApprovalPart = contentEvent.AuditTrailEventItem.ContentItem.As<ContentApprovalPart>();
                if (contentApprovalPart != null)
                {
                    if (contentEvent.Name.Equals(Constants.ContentEvent_Published, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ProcessPublishedEvent(contentEvent, contentApprovalPart);
                    }
                    else if (contentEvent.Name.Equals(Constants.ContentEvent_Saved, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ProcessSavedEvent(contentEvent, contentApprovalPart);
                    }
                }
            }

            return Task.CompletedTask;
        }

        private static void ProcessPublishedEvent(AuditTrailCreateContext<AuditTrailContentEvent> publishedContentEvent, ContentApprovalPart contentApprovalPart)
        {
            var formSubmitAction = contentApprovalPart.FormSubmitAction;
            if (formSubmitAction != null)
            {
                (string button, string action) = formSubmitAction;

                // Publish/Force Publish button
                if (!string.IsNullOrEmpty(action))
                {
                    switch (action)
                    {
                        case Constants.Action_Publish:
                            publishedContentEvent.Name = Constants.ContentEvent_Published;
                            break;

                        case Constants.Action_Publish_Continue:
                            publishedContentEvent.Name = Constants.ContentEvent_Publish_Continue;
                            break;

                        case Constants.Action_ForcePublish_ContentDesign:
                            publishedContentEvent.Name = GetPublishedEventName(contentApprovalPart, Constants.ReviewType_ContentDesign);
                            break;

                        case Constants.Action_ForcePublish_Stakeholder:
                            publishedContentEvent.Name = GetPublishedEventName(contentApprovalPart, Constants.ReviewType_Stakeholder);
                            break;

                        case Constants.Action_ForcePublish_Sme:
                            publishedContentEvent.Name = GetPublishedEventName(contentApprovalPart, Constants.ReviewType_Sme);
                            break;

                        case Constants.Action_ForcePublish_Ux:
                            publishedContentEvent.Name = GetPublishedEventName(contentApprovalPart, Constants.ReviewType_Ux);
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        private static void ProcessSavedEvent(AuditTrailCreateContext<AuditTrailContentEvent> savedContentEvent, ContentApprovalPart contentApprovalPart)
        {
            var formSubmitAction = contentApprovalPart.FormSubmitAction;
            if (formSubmitAction != null)
            {
                (string button, string action) = formSubmitAction;

                // Request Review/Save Draft/Save Draft & Exit button
                if (!string.IsNullOrEmpty(action))
                {
                    switch (action)
                    {
                        case Constants.Action_SaveDraft_Exit:
                            savedContentEvent.Name = Constants.ContentEvent_SaveDraft_Exit;
                            break;

                        case Constants.Action_SaveDraft_Continue:
                            savedContentEvent.Name = Constants.ContentEvent_SaveDraft_Continue;
                            break;

                        case Constants.Action_RequestReview_ContentDesign:
                            savedContentEvent.Name = GetSavedEventName(contentApprovalPart, Constants.ReviewType_ContentDesign);
                            break;

                        case Constants.Action_RequestReview_Stakeholder:
                            savedContentEvent.Name = GetSavedEventName(contentApprovalPart, Constants.ReviewType_Stakeholder);
                            break;

                        case Constants.Action_RequestReview_Sme:
                            savedContentEvent.Name = GetSavedEventName(contentApprovalPart, Constants.ReviewType_Sme);
                            break;

                        case Constants.Action_RequestReview_Ux:
                            savedContentEvent.Name = GetSavedEventName(contentApprovalPart, Constants.ReviewType_Ux);
                            break;

                        case Constants.Action_SendBack:
                            savedContentEvent.Name = Constants.ContentEvent_SendBack;
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        private static string GetPublishedEventName(ContentApprovalPart contentApprovalPart, string reviewType)
        {
            reviewType = reviewType.ToLower();

            string contentEventName = string.Empty;
            string forcePublishText = $"Force published ({reviewType})";
            string inReviewText = $"In review ({reviewType.ToLower()})";

            if (contentApprovalPart.IsForcePublished)
            {
                contentEventName = contentApprovalPart.ReviewStatus != Models.Enums.ReviewStatus.InReview
                ? forcePublishText
                : inReviewText;
            }
            else
            {
                contentEventName = GetReviewText(contentApprovalPart, reviewType);
            }

            return contentEventName;
        }

        private static string GetSavedEventName(ContentApprovalPart contentApprovalPart, string reviewType)
        {
            reviewType = reviewType.ToLower();
            return GetReviewText(contentApprovalPart, reviewType);
        }

        private static string GetReviewText(ContentApprovalPart contentApprovalPart, string reviewType)
        {
            string contentEventName = string.Empty;
            string requestReviewText = $"Requested for review ({reviewType})";
            string inReviewText = $"In review ({reviewType.ToLower()})";

            contentEventName = contentApprovalPart.ReviewStatus != Models.Enums.ReviewStatus.InReview
            ? requestReviewText
            : inReviewText;

            return contentEventName;
        }
    }
}
