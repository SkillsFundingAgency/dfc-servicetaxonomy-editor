namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    public static class Constants
    {
        public const string SubmitSaveKey = "submit.Save";
        public const string SubmitPublishKey = "submit.Publish";

        public const string SubmitRequiresRevisionValue = "submit.RequiresRevision";
        public const string SubmitRequestApprovalValuePrefix = "submit.RequestApproval-";
        public const string ContentApprovalPartPrefix = "ContentApproval";

        // Shared strings
        public const string ReviewType_ContentDesign = "content design";
        public const string ReviewType_Stakeholder = "stakeholder";
        public const string ReviewType_Sme = "sme";
        public const string ReviewType_Ux = "ux";

        public const string ReviewRequest_ContentDesign = "submit.RequestApproval-ContentDesign";
        public const string ReviewRequest_Stakeholder = "submit.RequestApproval-Stakeholder";
        public const string ReviewRequest_Sme = "submit.RequestApproval-SME";
        public const string ReviewRequest_Ux = "submit.RequestApproval-UX";

        // Content approval button actions
        public const string Action_Publish = SubmitPublishKey;
        public const string Action_Publish_Continue = "submit.PublishAndContinue";
        public const string Action_ForcePublish_ContentDesign = ReviewRequest_ContentDesign;
        public const string Action_ForcePublish_Stakeholder = ReviewRequest_Stakeholder;
        public const string Action_ForcePublish_Sme = ReviewRequest_Sme;
        public const string Action_ForcePublish_Ux = ReviewRequest_Ux;
        public const string Action_SaveDraft_Exit = SubmitSaveKey;
        public const string Action_SaveDraft_Continue = "submit.SaveAndContinue";
        public const string Action_RequestReview_ContentDesign = ReviewRequest_ContentDesign;
        public const string Action_RequestReview_Stakeholder = ReviewRequest_Stakeholder;
        public const string Action_RequestReview_Sme = ReviewRequest_Sme;
        public const string Action_RequestReview_Ux = ReviewRequest_Ux;
        public const string Action_SendBack = "submit.RequiresRevision";

        // Content event names
        public const string ContentEvent_Publish = "Published";
        public const string ContentEvent_Publish_Continue = "Published and continued";
        public const string ContentEvent_SaveDraft_Exit = "Draft saved and exited";
        public const string ContentEvent_SaveDraft_Continue = "Draft saved and continued";
        public const string ContentEvent_SendBack = "Sent back for revision";
    }
}
