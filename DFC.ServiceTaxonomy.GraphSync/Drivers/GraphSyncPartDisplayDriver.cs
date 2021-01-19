using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.ViewModels;
using OrchardCore.ContentManagement.Display.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Drivers
{
    public class GraphSyncPartDisplayDriver : ContentPartDisplayDriver<GraphSyncPart>
    {
        private readonly ISyncNameProvider _syncNameProvider;

        public GraphSyncPartDisplayDriver(ISyncNameProvider syncNameProvider)
        {
            _syncNameProvider = syncNameProvider;
        }

        // public override IDisplayResult Display(GraphSyncPart GraphSyncPart)
        // {
        //     return Combine(
        //         Initialize<GraphSyncPartViewModel>("GraphSyncPart", m => BuildViewModel(m, GraphSyncPart))
        //             .Location("Detail", "Content:20"),
        //         Initialize<GraphSyncPartViewModel>("GraphSyncPart_DetailAdmin", m => BuildViewModel(m, GraphSyncPart))
        //             .Location("DetailAdmin", "Content:20"),
        //         Initialize<GraphSyncPartViewModel>("GraphSyncPart_Summary", m => BuildViewModel(m, GraphSyncPart))
        //             .Location("Summary", "Meta:5")
        //     );
        // }

        public override IDisplayResult Display(GraphSyncPart GraphSyncPart, BuildPartDisplayContext context)
        {
            return Initialize<GraphSyncPartViewModel>(GetDisplayShapeType(context), async m => await BuildViewModel(m, GraphSyncPart)) //, context))
                    .Location("Detail", "Content:5")
                    .Location("Summary", "Content:10")
                    .Location("DetailAdmin", "Content:10") // For dashboard widgets;
                ;
        }
        public override IDisplayResult Edit(GraphSyncPart graphSyncPart) //, BuildPartEditorContext context)
        {
            return Initialize<GraphSyncPartViewModel>("GraphSyncPart_Edit",
                async m => await BuildViewModel(m, graphSyncPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(GraphSyncPart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Text);

            return Edit(model);
        }

        //todo: ValueTask
        private async Task BuildViewModel(GraphSyncPartViewModel model, GraphSyncPart part)
        {
            //todo: pass content type instead?
            _syncNameProvider.ContentType = part.ContentItem.ContentType;
            model.Text = part.Text ?? await _syncNameProvider.GenerateIdPropertyValue();
            model.Readonly = _syncNameProvider.GraphSyncPartSettings.PreexistingNode;
            model.DisplayId = _syncNameProvider.GraphSyncPartSettings.DisplayId;
        }
    }
}
