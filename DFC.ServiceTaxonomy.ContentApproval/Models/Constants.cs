namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
     public static class Constants
    {
        public const string contentdesign = "content design"; // case of constant matches the string casing
        public const string stakeholder = "stakeholder";
        public const string sme = "sme";
        public const string ux = "ux";


        public const string SubmitSaveKey = "submit.Save";
        public const string SubmitPublishKey = "submit.Publish";

        public const string SubmitRequiresRevisionValue = "submit.RequiresRevision";
        public const string SubmitRequestApprovalValuePrefix = "submit.RequestApproval-";
        public const string ContentApprovalPartPrefix = "ContentApproval";

        public const string SubmitPublishAction = SubmitPublishKey;
        public const string ContentEventPublished = "Published";

        public const string SubmitPublishAndContinueAction = "submit.PublishAndContinue";
        public const string ContentEventPublishedAndContinued = "Published and continued";

        public const string SubmitForcePublishContentDesignAction = "submit.RequestApproval-ContentDesign";
        public const string SubmitForcePublishedStakeholderAction = "submit.RequestApproval-Stakeholder";
        public const string SubmitForcePublishedSmeAction = "submit.RequestApproval-SME";
        public const string SubmitForcePublishedUxAction = "submit.RequestApproval-UX";

        public const string SubmitSaveAction = SubmitSaveKey;
        public const string ContentEventDraftSavedAndExited = "Draft saved and exited";

        public const string SubmitSaveAndContinueAction = "submit.SaveAndContinue";
        public const string ContentEventSavedAndContinued = "Draft saved and continued";

        public const string SubmitSaveAndRequestReviewContentDesignAction = "submit.RequestApproval-ContentDesign";
        public const string ContentEventRequestReviewContentDesign = contentdesign;

        public const string SubmitSaveAndRequestReviewStakeholderAction = "submit.RequestApproval-Stakeholder";
        public const string ContentEventRequestReviewStakeholder = stakeholder;

        public const string SubmitSaveAndRequestReviewSmeAction = "submit.RequestApproval-SME";
        public const string ContentEventRequestReviewSme = sme;

        public const string SubmitSaveAndRequestReviewUxAction = "submit.RequestApproval-UX";
        public const string ContentEventRequestReviewUx = ux;

        public const string SubmitSaveAndRequiresRevisionAction = "submit.RequiresRevision";
        public const string ContentEventRequiresRevision = "Sent back for revision";
    }
}
