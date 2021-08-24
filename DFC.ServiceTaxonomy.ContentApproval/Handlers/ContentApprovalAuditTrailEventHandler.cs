using System;
//using System.Linq;
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
            //string actionType = string.Empty;

            if (context is AuditTrailCreateContext<AuditTrailContentEvent> contentEvent)
            {
                var contentApprovalPartExists = contentEvent.AuditTrailEventItem.ContentItem.Has<ContentApprovalPart>();
                if (contentApprovalPartExists)
                {
                    var contentApprovalPart = contentEvent.AuditTrailEventItem.ContentItem.As<ContentApprovalPart>();
                    if (contentApprovalPart != null)
                    {
                        var formSubmitAction = contentApprovalPart.FormSubmitAction;
                        if(formSubmitAction != null)
                        {
                            (string button, string action) = formSubmitAction;

                            // Publish/Force Publish button
                            if (button.Contains(Constants.SubmitPublishKey))
                            {
                                switch (action)
                                {
                                    case Constants.SubmitPublishAction:
                                        contentEvent.Name = Constants.ContentEventPublished;
                                        break;

                                    case Constants.SubmitPublishAndContinueAction:
                                        contentEvent.Name = Constants.ContentEventPublishedAndContinued;
                                        break;

                                    case Constants.SubmitForcePublishContentDesignAction:
                                        contentEvent.Name = GetContentEventName(contentApprovalPart, Constants.ContentEventRequestReviewContentDesign);
                                        break;

                                    case Constants.SubmitForcePublishedStakeholderAction:
                                        contentEvent.Name = GetContentEventName(contentApprovalPart, Constants.ContentEventRequestReviewStakeholder);
                                        break;

                                    case Constants.SubmitForcePublishedSmeAction:
                                        contentEvent.Name = GetContentEventName(contentApprovalPart, Constants.ContentEventRequestReviewSme);
                                        break;

                                    case Constants.SubmitForcePublishedUxAction:
                                        contentEvent.Name = GetContentEventName(contentApprovalPart, Constants.ContentEventRequestReviewUx);
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
                                    case Constants.SubmitSaveKey:
                                        contentEvent.Name = Constants.ContentEventDraftSavedAndExited;
                                        break;

                                    case Constants.SubmitSaveAndContinueAction:
                                        contentEvent.Name = Constants.ContentEventSavedAndContinued;
                                        break;

                                    case Constants.SubmitSaveAndRequestReviewContentDesignAction:
                                        contentEvent.Name = GetContentEventName(contentApprovalPart, Constants.ContentEventRequestReviewContentDesign);
                                        break;

                                    case Constants.SubmitSaveAndRequestReviewStakeholderAction:
                                        contentEvent.Name = GetContentEventName(contentApprovalPart, Constants.ContentEventRequestReviewStakeholder);
                                        break;

                                    case Constants.SubmitSaveAndRequestReviewSmeAction:
                                        contentEvent.Name = GetContentEventName(contentApprovalPart, Constants.ContentEventRequestReviewSme);
                                        break;

                                    case Constants.SubmitSaveAndRequestReviewUxAction:
                                        contentEvent.Name = GetContentEventName(contentApprovalPart, Constants.ContentEventRequestReviewUx);
                                        break;

                                    case Constants.SubmitSaveAndRequiresRevisionAction:
                                        contentEvent.Name = Constants.ContentEventRequiresRevision;
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

        private static string GetContentEventName(ContentApprovalPart contentApprovalPart, string reviewType)
        {
            string contentEventName = string.Empty;

            // Force published button - Content Design, Stakeholder, SME, UX
            if(contentApprovalPart.IsForcePublished)
            {
               contentEventName = contentApprovalPart.ReviewStatus != Models.Enums.ReviewStatus.InReview
               ? $"Force published ({reviewType})"
               : $"In review ({reviewType})";
            }
            // Review request button - Content Design, Stakeholder, SME, UX
            else
            {
               contentEventName = contentApprovalPart.ReviewStatus != Models.Enums.ReviewStatus.InReview
               ? $"Requested for review ({reviewType})"
               : $"In review ({reviewType})";
            }

            return contentEventName;
        }
    }
}
