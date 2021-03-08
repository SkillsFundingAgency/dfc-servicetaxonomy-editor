using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

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
                newDraftItem.Alter<ContentApprovalPart>(c => c.ReviewStatus = ContentReviewStatus.ReadyForReview);
                await _contentManager.SaveDraftAsync(newDraftItem);
            }
        }
    }
}
