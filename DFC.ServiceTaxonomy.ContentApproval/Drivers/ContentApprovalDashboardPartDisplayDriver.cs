using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.ContentApproval.Drivers
{
    class ContentApprovalDashboardPartDisplayDriver : ContentPartDisplayDriver<ContentApprovalDashboardPart>
    {
        public ContentApprovalDashboardPartDisplayDriver()
        {
        }

        public override Task<IDisplayResult> DisplayAsync(ContentApprovalDashboardPart part, BuildPartDisplayContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<ContentApprovalDashboardPartViewModel>(
                    GetDisplayShapeType(context), async m => await BuildViewModel()) //m, part)) //, context))
                    .Location("DetailAdmin", "Content:10"));
        }

        // public override Task<IDisplayResult> EditAsync(ContentApprovalDashboardPart part, BuildPartEditorContext context)
        // {
        //     return Task.FromResult<IDisplayResult>(
        //         Initialize<ContentApprovalDashboardPartViewModel>(GetEditorShapeType(context), m => BuildViewModel(m, part)));
        // }
        //
        // public override async Task<IDisplayResult> UpdateAsync(ContentApprovalDashboardPart model, IUpdateModel updater, UpdatePartEditorContext context)
        // {
        //     await updater.TryUpdateModelAsync(model, Prefix, t => t.ViewType);
        //
        //     return await EditAsync(model, context);
        // }

        private ValueTask BuildViewModel() //ContentApprovalDashboardPartViewModel model, ContentApprovalDashboardPart part)
        {
            // model.ViewType = part.ViewType;
            // model.DashboardPart = part;
            // model.ContentItem = part.ContentItem;

            return default;
        }
    }
}
