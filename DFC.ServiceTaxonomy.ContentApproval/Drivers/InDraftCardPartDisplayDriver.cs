using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.ContentApproval.Drivers
{
    class InDraftCardPartDisplayDriver : ContentPartDisplayDriver<InDraftCardPart>
    {
        public override Task<IDisplayResult> DisplayAsync(InDraftCardPart part, BuildPartDisplayContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<InDraftCardPartViewModel>(
                        GetDisplayShapeType(context), async m => await BuildViewModel()) //m, part)) //, context))
                    .Location("DetailAdmin", "Content:10"));
        }

        private ValueTask
            BuildViewModel() //ContentApprovalDashboardPartViewModel model, ContentApprovalDashboardPart part)
        {
            // model.ViewType = part.ViewType;
            // model.DashboardPart = part;
            // model.ContentItem = part.ContentItem;

            return default;
        }
    }
}
