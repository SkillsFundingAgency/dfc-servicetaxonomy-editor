namespace DFC.ServiceTaxonomy.CustomEditor.Constants
{
    public static class AuditTrail
    {
        // submit button values
        public const string Save_Button_Key = "submit.Save";
        public const string Publish_Button_Key = "submit.Publish";
        public const string Cancel_Publish_Later = "submit.CancelPublishLater";
        public const string Cancel_Unpublish_Later = "submit.CancelUnpublishLater";
        public const string ContentApproval_Requires_Revision = "submit.RequiresRevision";
        public const string ContentApproval_Request_Approval = "submit.RequestApproval-";


        public const string ContentEvent_Removed = "Removed";
        public const string ContentEventName_DiscardDraft = "Discard draft";
        public const string UrlPart_DiscardDraft = "discarddraft";

        // Content event names
        // Standard
        public const string ContentEvent_Published = "Published";
        public const string ContentEvent_Unpublished = "Unpublished";
        public const string ContentEvent_Saved = "Saved";

        // Unpublish later
        public const string ContentEvent_Unpublish_Later_Cancelled = "Unpublish later cancelled";
        public const string ContentEvent_Unpublish_Later = "Unpublish later";
        public const string ContentEvent_Unpublish_Later_Success = "Unpublish later  success";

        // Publish later
        public const string ContentEvent_Publish_Later_Cancelled = "Publish later cancelled";
        public const string ContentEvent_Publish_Later = "Publish later";
        public const string ContentEvent_Publish_Later_Success = "Publish later success";

        // Content Approval
        public const string ReviewType_ContentDesign = "content design";
        public const string ReviewType_Stakeholder = "stakeholder";
        public const string ReviewType_Sme = "sme";
        public const string ReviewType_Ux = "ux";
        public const string ContentEventName_SendBack = "Sent back for revision";

    }
}
