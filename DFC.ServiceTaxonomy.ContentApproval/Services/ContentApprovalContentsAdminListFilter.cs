using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Indexes;
using DFC.ServiceTaxonomy.ContentApproval.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;

namespace DFC.ServiceTaxonomy.ContentApproval.Services
{
    public class ContentApprovalContentsAdminListFilter : IContentsAdminListFilter
    {
        // private readonly IContentDefinitionManager _contentDefinitionManager;
        //
        // public ContentApprovalContentsAdminListFilter(IContentDefinitionManager contentDefinitionManager)
        // {
        //     _contentDefinitionManager = contentDefinitionManager;
        // }

        public async Task FilterAsync(ContentOptionsViewModel model, IQuery<ContentItem> query, IUpdateModel updater)
        {
            var viewModel = new ContentApprovalContentsAdminListFilterViewModel();
            //todo: do we need to set Prefix?
            // ContentApproval.SelectedApprovalStatus ??
            //todo: common const for prefix
            if (await updater.TryUpdateModelAsync(viewModel, "ContentApproval"))
            {
                // Show localization content items
                // This is intended to be used by adding ?Localization.ShowLocalizedContentTypes to an AdminMenu url.
                // if (viewModel.ShowLocalizedContentTypes)
                // {
                //     var localizedTypes = _contentDefinitionManager
                //         .ListTypeDefinitions()
                //         .Where(x =>
                //             x.Parts.Any(p =>
                //                 p.PartDefinition.Name == nameof(ContentApprovalPart)))
                //         .Select(x => x.Name);
                //
                //     query.With<ContentItemIndex>(x => x.ContentType.IsIn(localizedTypes));
                // }
                // else
                // if (!String.IsNullOrEmpty(viewModel.SelectedApprovalStatus))
                // {
                    //todo: do we need published and latest?
                    //query.With<ContentApprovalPartIndex>(i => (i.Published || i.Latest) && i.Culture == viewModel.SelectedCulture);

                    //

                    query.With<ContentApprovalPartIndex>(i => i.ReviewStatus == viewModel.SelectedApprovalStatus);
                // }
            }
        }
    }
}
