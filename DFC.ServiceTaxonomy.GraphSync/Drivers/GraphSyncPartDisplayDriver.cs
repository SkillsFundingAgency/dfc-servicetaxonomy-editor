using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using DFC.ServiceTaxonomy.GraphSync.ViewModels;

namespace DFC.ServiceTaxonomy.GraphSync.Drivers
{
    public class GraphSyncPartDisplayDriver : ContentPartDisplayDriver<GraphSyncPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public GraphSyncPartDisplayDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(GraphSyncPart GraphSyncPart)
        {
            return Combine(
                Initialize<GraphSyncPartViewModel>("GraphSyncPart", m => BuildViewModel(m, GraphSyncPart))
                    .Location("Detail", "Content:20"),
                Initialize<GraphSyncPartViewModel>("GraphSyncPart_Summary", m => BuildViewModel(m, GraphSyncPart))
                    .Location("Summary", "Meta:5")
            );
        }
        
        public override IDisplayResult Edit(GraphSyncPart GraphSyncPart)
        {
            return Initialize<GraphSyncPartViewModel>("GraphSyncPart_Edit", m => BuildViewModel(m, GraphSyncPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(GraphSyncPart model, IUpdateModel updater)
        {
            var settings = GetGraphSyncPartSettings(model);

            await updater.TryUpdateModelAsync(model, Prefix, t => t.Show);
            
            return Edit(model);
        }

        public GraphSyncPartSettings GetGraphSyncPartSettings(GraphSyncPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(GraphSyncPart));
            var settings = contentTypePartDefinition.GetSettings<GraphSyncPartSettings>();

            return settings;
        }

        private Task BuildViewModel(GraphSyncPartViewModel model, GraphSyncPart part)
        {
            var settings = GetGraphSyncPartSettings(part);

            model.ContentItem = part.ContentItem;
            model.MySetting = settings.MySetting;
            model.Show = part.Show;
            model.GraphSyncPart = part;
            model.Settings = settings;

            return Task.CompletedTask;
        }
    }
}
