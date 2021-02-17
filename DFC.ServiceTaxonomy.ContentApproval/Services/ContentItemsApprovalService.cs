using DFC.ServiceTaxonomy.Content.Services.Interface;
using DFC.ServiceTaxonomy.ContentApproval.Models;

namespace DFC.ServiceTaxonomy.ContentApproval.Services
{
    //todo: add counts for standard statuses into IContentItemsService
    // and add counts (fetches too?) for content approval only statuses in here
    public class ContentItemsApprovalService : IContentItemsApprovalService
    {
        private readonly IContentItemsService _contentItemsService;

        public ContentItemsApprovalService(IContentItemsService contentItemsService)
        {
            _contentItemsService = contentItemsService;
        }

        //todo: remove card from enum?
        public int ItemCount(DashboardItemsStatusCard itemStatus)
        {
            //todo: maybe not have switch - named instances or something?
            switch (itemStatus)
            {
                case DashboardItemsStatusCard.InDraft:
                    //todo:
                    return 72;
                default:
                    return 98;
            }
        }
    }
}
