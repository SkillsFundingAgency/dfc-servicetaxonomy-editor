using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Parts;
using DFC.ServiceTaxonomy.Editor.Module.Settings;
using DFC.ServiceTaxonomy.Editor.Module.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.Editor.Module.Drivers
{
    public class GraphLookupPartDisplayDriver : ContentPartDisplayDriver<GraphLookupPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public GraphLookupPartDisplayDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Edit(GraphLookupPart part)
        {
            return Initialize<GraphLookupPartViewModel>("GraphLookupPart_Edit", m => BuildViewModel(m, part));
        }

        public override async Task<IDisplayResult> UpdateAsync(GraphLookupPart part, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(part, Prefix, t => t.Value, t => t.DisplayText);

            return Edit(part);
        }

        public GraphLookupPartSettings GetAliasPartSettings(GraphLookupPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(GraphLookupPart));
            return contentTypePartDefinition.GetSettings<GraphLookupPartSettings>();
        }

        private void BuildViewModel(GraphLookupPartViewModel model, GraphLookupPart part)
        {
            var settings = GetAliasPartSettings(part);

            model.Value = part.Value;
            model.DisplayText = part.DisplayText;
            model.GraphLookupPart = part;
            model.Settings = settings;
        }
    }
}
