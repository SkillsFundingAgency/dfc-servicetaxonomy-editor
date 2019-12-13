using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using DFC.ServiceTaxonomy.GraphLookup.Models;
using DFC.ServiceTaxonomy.GraphLookup.Settings;
using DFC.ServiceTaxonomy.GraphLookup.ViewModels;

namespace DFC.ServiceTaxonomy.GraphLookup.Drivers
{
    public class GraphLookupPartDisplayDriver : ContentPartDisplayDriver<GraphLookupPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public GraphLookupPartDisplayDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(GraphLookupPart GraphLookupPart)
        {
            return Combine(
                Initialize<GraphLookupPartViewModel>("GraphLookupPart", m => BuildViewModel(m, GraphLookupPart))
                    .Location("Detail", "Content:20"),
                Initialize<GraphLookupPartViewModel>("GraphLookupPart_Summary", m => BuildViewModel(m, GraphLookupPart))
                    .Location("Summary", "Meta:5")
            );
        }
        
        public override IDisplayResult Edit(GraphLookupPart GraphLookupPart)
        {
            return Initialize<GraphLookupPartViewModel>("GraphLookupPart_Edit", m => BuildViewModel(m, GraphLookupPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(GraphLookupPart model, IUpdateModel updater)
        {
            var settings = GetGraphLookupPartSettings(model);

            await updater.TryUpdateModelAsync(model, Prefix, t => t.Show);
            
            return Edit(model);
        }

        public GraphLookupPartSettings GetGraphLookupPartSettings(GraphLookupPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(GraphLookupPart));
            var settings = contentTypePartDefinition.GetSettings<GraphLookupPartSettings>();

            return settings;
        }

        private Task BuildViewModel(GraphLookupPartViewModel model, GraphLookupPart part)
        {
            var settings = GetGraphLookupPartSettings(part);

            model.ContentItem = part.ContentItem;
            model.MySetting = settings.MySetting;
            model.Show = part.Show;
            model.GraphLookupPart = part;
            model.Settings = settings;

            return Task.CompletedTask;
        }
    }
}
