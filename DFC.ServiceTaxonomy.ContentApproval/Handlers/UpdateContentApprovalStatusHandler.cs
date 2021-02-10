using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.ContentApproval.Handlers
{
    public class UpdateContentApprovalStatusHandler : ContentHandlerBase
    {
        public override Task DraftSavingAsync(SaveDraftContentContext context)
        {
            var contentApprovalPart = context.ContentItem.As<ContentApprovalPart>();
            contentApprovalPart.ApprovalStatus = ContentApprovalStatus.InDraft;
            contentApprovalPart.Apply();
            return base.DraftSavedAsync(context);
        }

        //public override Task DraftSavedAsync(SaveDraftContentContext context)
        //{
        //    var contentApprovalPart = context.ContentItem.As<ContentApprovalPart>();
        //    contentApprovalPart.ApprovalStatus = ContentApprovalStatus.InDraft;
        //    contentApprovalPart.Apply();
        //    return base.DraftSavedAsync(context);
        //}
    }
}
