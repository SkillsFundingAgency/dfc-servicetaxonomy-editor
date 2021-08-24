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
                                    case "submit.Publish":
                                        contentEvent.Name = "Published";
                                        break;

                                    case "submit.PublishAndContinue":
                                        contentEvent.Name = "Published and continued";
                                        break;

                                    case "submit.RequestApproval-ContentDesign":
                                        contentEvent.Name = "Force published (ContentDesign)";
                                        break;

                                    case "submit.RequestApproval-Stakeholder":
                                        contentEvent.Name = "Force published (Stakeholder)";
                                        break;

                                    case "submit.RequestApproval-SME":
                                        contentEvent.Name = "Force published (SME)";
                                        break;

                                    case "submit.RequestApproval-UX":
                                        contentEvent.Name = "Force published (UX)";
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
                                    case "submit.Save":
                                        contentEvent.Name = "Draft saved and exited";
                                        break;

                                    case "submit.SaveAndContinue":
                                        contentEvent.Name = "Draft saved and continued";
                                        break;

                                    case "submit.RequestApproval-ContentDesign":
                                        contentEvent.Name = GetContentEventName(contentEvent, contentApprovalPart, "content design");
                                        break;

                                    case "submit.RequestApproval-Stakeholder":
                                        contentEvent.Name = GetContentEventName(contentEvent, contentApprovalPart, "stakeholder");
                                        break;

                                    case "submit.RequestApproval-SME":
                                        contentEvent.Name = GetContentEventName(contentEvent, contentApprovalPart, "sme");
                                        break;

                                    case "submit.RequestApproval-UX":
                                        contentEvent.Name = GetContentEventName(contentEvent, contentApprovalPart, "ux");
                                        break;

                                    case "submit.RequiresRevision":
                                        contentEvent.Name = "Sent back for revision";
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

        private static string GetContentEventName(AuditTrailCreateContext<AuditTrailContentEvent> contentEvent, ContentApprovalPart contentApprovalPart, string reviewType)
        {
            contentEvent.Name = contentApprovalPart.ReviewStatus != Models.Enums.ReviewStatus.InReview
                ? $"Requested for review ({reviewType})"
                : $"In review ({reviewType})";

            return contentEvent.Name;
        }
    }
}
