using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.ContentApproval.Drivers
{
    public class ContentApprovalPartDisplayDriver : ContentPartDisplayDriver<ContentApprovalPart>
    {
        public override IDisplayResult Display(ContentApprovalPart part, BuildPartDisplayContext context) =>
            Initialize<ContentApprovalPartViewModel>(
                GetDisplayShapeType(context),
                viewModel => PopulateViewModel(part, viewModel))
            //.Location("Detail", "Content:5")
            .Location("Summary", "Actions:First");


        public override IDisplayResult Edit(ContentApprovalPart part, BuildPartEditorContext context)
        {
            return Initialize<ContentApprovalPartViewModel>(
                    GetEditorShapeType(context),
                    viewModel => PopulateViewModel(part, viewModel))
                .Location("Actions:First");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentApprovalPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new ContentApprovalPartViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            // Populate part from view model here.Hi 

            return await EditAsync(part, context);
        }

        private static void PopulateViewModel(ContentApprovalPart part, ContentApprovalPartViewModel viewModel)
        {
            viewModel.ApprovalStatus = part.ApprovalStatus;
            viewModel.Comment = part.Comment;
        }
    }
}
