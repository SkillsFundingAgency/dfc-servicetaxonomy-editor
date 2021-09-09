﻿using System;
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

            if (context is AuditTrailCreateContext<AuditTrailContentEvent> contentEvent && !contentEvent.Name.Equals(Constants.ContentEvent_Created, StringComparison.InvariantCultureIgnoreCase))
            {
                var contentApprovalPartExists = contentEvent.AuditTrailEventItem.ContentItem.Has<ContentApprovalPart>();
                if (contentApprovalPartExists)
                {
                    var contentApprovalPart = contentEvent.AuditTrailEventItem.ContentItem.As<ContentApprovalPart>();
                    if (contentApprovalPart != null)
                    {
                        var formSubmitAction = contentApprovalPart.FormSubmitAction;
                        if (formSubmitAction != null)
                        {
                            (string button, string action) = formSubmitAction;

                            // Publish/Force Publish button
                            if (button.Contains(Constants.SubmitPublishKey))
                            {
                                switch (action)
                                {
                                    case Constants.Action_Publish:
                                        contentEvent.Name = Constants.ContentEvent_Publish;
                                        break;

                                    case Constants.Action_Publish_Continue:
                                        contentEvent.Name = Constants.ContentEvent_Publish_Continue;
                                        break;

                                    case Constants.Action_ForcePublish_ContentDesign:
                                        contentEvent.Name = GetContentEventName(contentApprovalPart, button, Constants.ReviewType_ContentDesign);
                                        break;

                                    case Constants.Action_ForcePublish_Stakeholder:
                                        contentEvent.Name = GetContentEventName(contentApprovalPart, button, Constants.ReviewType_Stakeholder);
                                        break;

                                    case Constants.Action_ForcePublish_Sme:
                                        contentEvent.Name = GetContentEventName(contentApprovalPart, button, Constants.ReviewType_Sme);
                                        break;

                                    case Constants.Action_ForcePublish_Ux:
                                        contentEvent.Name = GetContentEventName(contentApprovalPart, button, Constants.ReviewType_Ux);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            // Request Review/Save Draft/Save Draft & Exit button
                            else if (button.Contains(Constants.SubmitSaveKey))
                            {
                                switch (action)
                                {
                                    case Constants.Action_SaveDraft_Exit:
                                        contentEvent.Name = Constants.ContentEvent_SaveDraft_Exit;
                                        break;

                                    case Constants.Action_SaveDraft_Continue:
                                        contentEvent.Name = Constants.ContentEvent_SaveDraft_Continue;
                                        break;

                                    case Constants.Action_RequestReview_ContentDesign:
                                        contentEvent.Name = GetContentEventName(contentApprovalPart, button, Constants.ReviewType_ContentDesign);
                                        break;

                                    case Constants.Action_RequestReview_Stakeholder:
                                        contentEvent.Name = GetContentEventName(contentApprovalPart, button, Constants.ReviewType_Stakeholder);
                                        break;

                                    case Constants.Action_RequestReview_Sme:
                                        contentEvent.Name = GetContentEventName(contentApprovalPart, button, Constants.ReviewType_Sme);
                                        break;

                                    case Constants.Action_RequestReview_Ux:
                                        contentEvent.Name = GetContentEventName(contentApprovalPart, button, Constants.ReviewType_Ux);
                                        break;

                                    case Constants.Action_SendBack:
                                        contentEvent.Name = Constants.ContentEvent_SendBack;
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        private static string GetContentEventName(ContentApprovalPart contentApprovalPart, string button, string reviewType)
        {
            reviewType = reviewType.ToLower();

            string contentEventName = string.Empty;
            string forcePublishText = $"Force published ({reviewType})";
            string requestReviewText = $"Requested for review ({reviewType})";
            string inReviewText = $"In review ({reviewType.ToLower()})";

            if (button.Contains(Constants.SubmitPublishKey))
            {
                if (contentApprovalPart.IsForcePublished)
                {
                    contentEventName = contentApprovalPart.ReviewStatus != Models.Enums.ReviewStatus.InReview
                    ? forcePublishText
                    : inReviewText;
                }
                else
                {
                    contentEventName = contentApprovalPart.ReviewStatus != Models.Enums.ReviewStatus.InReview
                    ? requestReviewText
                    : inReviewText;
                }
            }
            else if (button.Contains(Constants.SubmitSaveKey))
            {
                contentEventName = contentApprovalPart.ReviewStatus != Models.Enums.ReviewStatus.InReview
                ? requestReviewText
                : inReviewText;
            }

            return contentEventName;
        }
    }
}
