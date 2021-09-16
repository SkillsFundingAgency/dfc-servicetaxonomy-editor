using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.Models.Enums;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Contents.AuditTrail.Models;

namespace DFC.ServiceTaxonomy.ContentApproval.Handlers
{
    public class ContentApprovalContentHandler: ContentPartHandler<ContentApprovalPart>
    {
        private readonly IContentManager _contentManager;

        public ContentApprovalContentHandler(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public override async Task PublishedAsync(PublishContentContext context, ContentApprovalPart instance)
        {
            if (instance.IsForcePublished)
            {
                var newDraftItem = await _contentManager.GetAsync(context.PublishingItem.ContentItemId,
                    VersionOptions.DraftRequired);
                newDraftItem.Author = context.PublishingItem.Author;
                newDraftItem.Alter<ContentApprovalPart>(c => c.ReviewStatus = ReviewStatus.ReadyForReview);
                if (newDraftItem.Has<AuditTrailPart>())
                {
                    newDraftItem.Alter<AuditTrailPart>(a => a.Comment = String.Empty);
                }
                await _contentManager.SaveDraftAsync(newDraftItem);
            }
        }
    }
}
