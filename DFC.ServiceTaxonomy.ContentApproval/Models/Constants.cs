namespace DFC.ServiceTaxonomy.ContentApproval.Models
{
    public static class Constants
    {
        public const string SubmitSaveKey = "submit.Save";
        public const string SubmitPublishKey = "submit.Publish";

        public const string SubmitRequiresRevisionValue = "submit.RequiresRevision";
        public const string SubmitRequestApprovalValuePrefix = "submit.RequestApproval-";
        public const string ContentApprovalPartPrefix = "ContentApproval";

        // Shared strings for review types
        public const string ReviewType_ContentDesign = "content design";
        public const string ReviewType_Stakeholder = "stakeholder";
        public const string ReviewType_Sme = "sme";
        public const string ReviewType_Ux = "ux";

        // Content event names
        public const string ContentEvent_Published = "Published";
        public const string ContentEvent_Saved = "Saved";

        // Content event labels
        public const string ContentEventName_SendBack = "Sent back for revision";
    }
}
