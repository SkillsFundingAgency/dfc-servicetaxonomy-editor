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
            #region comments

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
            

            #endregion

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
                savedContentEvent.Name = Constants.ContentEvent_SendBack;
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
            reviewType = reviewType.ToLower();
            return $"Force published ({reviewType.ToLower()})";
        }

        private static string GetSavedEventName(ContentApprovalPart contentApprovalPart, string reviewType)
        {
            reviewType = reviewType.ToLower();
            return GetReviewText(contentApprovalPart, reviewType);
        }

        private static string GetReviewText(ContentApprovalPart contentApprovalPart, string reviewType)
        {
            string requestReviewText = $"Requested for review ({reviewType})";
            string inReviewText = $"In review ({reviewType.ToLower()})";

            return contentApprovalPart.ReviewStatus != Models.Enums.ReviewStatus.InReview
            ? requestReviewText
            : inReviewText;
        }
    }
}
