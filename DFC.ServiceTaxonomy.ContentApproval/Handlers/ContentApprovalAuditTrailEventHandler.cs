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
        /* 
         * Form button map
         * 
         * Button                                           ContentEvent
         * ----------------------------------------------   ---------------------------------
         * Publish
         * Force-Publish
         * Review
         * Send Back                                        Saved
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         */



        public Task CreateAsync2(AuditTrailCreateContext context)
        {
            if (context is AuditTrailCreateContext<AuditTrailContentEvent> contentEvent)
            {
                var contentApprovalPartExists = contentEvent.AuditTrailEventItem.ContentItem.Has<ContentApprovalPart>();
                if (contentApprovalPartExists)
                {
                    var contentApprovalPart = contentEvent.AuditTrailEventItem.ContentItem.As<ContentApprovalPart>();
                    if (contentApprovalPart != null)
                    {
#pragma warning disable S1481 // Unused local variables should be removed
                        var reviewType = contentApprovalPart.ReviewType.ToString();
                        var reviewStatus = contentApprovalPart.ReviewStatus;
                        var updateModel = contentApprovalPart.updateModel;

#pragma warning restore S1481 // Unused local variables should be removed

#pragma warning disable S3923 // All branches in a conditional structure should not have exactly the same implementation
                        switch (context.Name)
#pragma warning restore S3923 // All branches in a conditional structure should not have exactly the same implementation
                        {
                            case "Saved":
                            //    if (reviewStatus == ReviewStatus.ReadyForReview && !contentApprovalPart.IsForcePublished)
                            //    {
                            //        contentEvent.Name = $"Request review for {reviewType}";
                            //    }
                            //    else if (reviewStatus == ReviewStatus.NotInReview && !contentApprovalPart.IsForcePublished)
                            //    {
                            //        contentEvent.Name = "Send back";
                            //    }
                            //    else if (contentApprovalPart.IsForcePublished)
                            //    {
                            //        contentEvent.Name = $"Force-Published draft automatically created";
                            //    }
                            //    break;

                            //case "Published":
                            //    if(contentApprovalPart.IsForcePublished)
                            //    {
                            //        contentEvent.Name = $"Force-Published for {reviewType}";
                            //    }
                                break;

                            case "Unpublished":
                                // No change of event name required for unpublished event
                                break;

                            case "RequestReview":
                                // No change of event name required for unpublished event
                                break;

                            case "SendBack":
                                // No change of event name required for unpublished event
                                break;

                            default:
                                break;
                        }

//                        // Force publish
//#pragma warning disable S1066 // Collapsible "if" statements should be merged
//                        if (context.Name == "Published" && contentApprovalPart.IsForcePublished)
//#pragma warning restore S1066 // Collapsible "if" statements should be merged
//                        {
//                            contentEvent.Name = $"Force-Published for {reviewType}";
//                        }
//                        // Request review
//                        else if (contentApprovalPart.ReviewStatus == ReviewStatus.ReadyForReview)
//                        {
//                            contentEvent.Name = $"Request review for {reviewType}";
//                        }
//                        // Review
//                        else if (contentApprovalPart.ReviewStatus == ReviewStatus.InReview)
//                        {
//                            contentEvent.Name = $"Review for {reviewType}";
//                        }




                        //if (contentApprovalPart.IsForcePublished)
                        //{
                        //    var reviewType = contentApprovalPart.ReviewType.ToString();
                        //    contentEvent.Name = $"Force-Publish.{reviewType}";
                        //}

                        //if (contentApprovalPart.ReviewStatus == ReviewStatus.InReview)
                        //{
                        //    //if(auditTrailPart.)
                        //    //{

                        //    //}
                        //    contentEvent.Name = "In review";
                        //}
                        //if (contentApprovalPart.ReviewStatus == ReviewStatus.ReadyForReview)
                        //{
                        //    contentEvent.Name = "Submitted for review";
                        //}

                        //if (context.Name == "Published" && contentApprovalPart.IsForcePublished)
                        //{
                        //    contentEvent.Name = "Force published";
                        //}
                    }
                }

                //var auditTrailPartExists = contentEvent.AuditTrailEventItem.ContentItem.Has<OrchardCore.Contents.AuditTrail.Models.AuditTrailPart>();
                //if (auditTrailPartExists)
                //{
                //    auditTrailPart = contentEvent.AuditTrailEventItem.ContentItem?.As<OrchardCore.Contents.AuditTrail.Models.AuditTrailPart>();
                //}


                //if (contentApprovalPart == null)
                //{
                //    return Task.CompletedTask;
                //}

                //if (contentApprovalPart.ReviewStatus == ReviewStatus.InReview)
                //{
                //    contentEvent.Name = "In review";
                //}
                //if (contentApprovalPart.ReviewStatus == ReviewStatus.ReadyForReview)
                //{
                //    contentEvent.Name = "Submitted for review";
                //}

                //if (context.Name == "Published" && contentApprovalPart.IsForcePublished)
                //{
                //    contentEvent.Name = "Force published";
                //}
            }

            return Task.CompletedTask;
        }


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
            string actionType = string.Empty;

            if (context is AuditTrailCreateContext<AuditTrailContentEvent> contentEvent)
            {
                var contentApprovalPartExists = contentEvent.AuditTrailEventItem.ContentItem.Has<ContentApprovalPart>();
                if (contentApprovalPartExists)
                {
                    var contentApprovalPart = contentEvent.AuditTrailEventItem.ContentItem.As<ContentApprovalPart>();
                    if (contentApprovalPart != null)
                    {
                        var updateModel = contentApprovalPart.updateModel;
                        if (updateModel != null && updateModel.ModelState != null && updateModel.ModelState?.Keys != null)
                        {
                            var keys = updateModel.ModelState.Keys;
                            if (keys.Any())
                            {
                                //----------------------------------------------
                                // Publish/Force-Publish button
                                //----------------------------------------------
                                if (keys.Contains(Constants.SubmitPublishKey))
                                {
                                    actionType = updateModel.ModelState[Constants.SubmitPublishKey].AttemptedValue;
                                    switch(actionType)
                                    {
                                        case "submit.RequestApproval-ContentDesign ":
                                            contentEvent.Name = $"Force-Published for Content Design";
                                            break;

                                        case "submit.RequestApproval-Stakeholder ":
                                            contentEvent.Name = $"Force-Published for Stakeholder";
                                            break;

                                        case "submit.RequestApproval-SME ":
                                            contentEvent.Name = $"Force-Published for SME";
                                            break;

                                        case "submit.RequestApproval-UX ":
                                            contentEvent.Name = $"Force-Published for UX";
                                            break;

                                        default:
                                            break;
                                    }
                                }
                                //----------------------------------------------
                                // Save Draft/Save Draft & Exit button
                                //----------------------------------------------
                                else if (keys.Contains(Constants.SubmitSaveKey))
                                {
                                    actionType = updateModel.ModelState[Constants.SubmitSaveKey].AttemptedValue;
#pragma warning disable S3923 // All branches in a conditional structure should not have exactly the same implementation
                                    switch (actionType)
#pragma warning restore S3923 // All branches in a conditional structure should not have exactly the same implementation
                                    {
                                        case "submit.Save":
                                        case "submit.RequiresRevision":
                                            break;

                                        case "submit.SaveAndContinue":
                                        case "submit.RequestApproval-ContentDesign":
                                        case "submit.RequestApproval-Stakeholder":
                                        case "submit.RequestApproval-SME":
                                        case "submit.RequestApproval-UX":
                                            break;

                                        default:
                                            break;
                                    }
                                }
                                //----------------------------------------------
                                // Request review button
                                //----------------------------------------------
                                else if (keys.Contains(Constants.SubmitRequestApprovalValuePrefix))
                                {
                                    contentEvent.Name = updateModel.ModelState[Constants.SubmitRequestApprovalValuePrefix].AttemptedValue;
                                }
                                //----------------------------------------------
                                // Send back button
                                //----------------------------------------------
                                else if (keys.Contains(Constants.SubmitRequiresRevisionValue))
                                {
                                    contentEvent.Name = updateModel.ModelState[Constants.SubmitRequiresRevisionValue].AttemptedValue;
                                }
                            }
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

    }
}
