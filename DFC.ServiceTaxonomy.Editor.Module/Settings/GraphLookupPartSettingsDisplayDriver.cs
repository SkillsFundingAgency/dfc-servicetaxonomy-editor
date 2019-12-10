using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.Editor.Module.Settings
{
    public class GraphLookupPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            //todo: is check strictly necessary? if so can't return null, can't change signature. throw?
            //if (!string.Equals(nameof(GraphLookupPart), contentTypePartDefinition.PartDefinition.Name))
            //{
            //    return default;
            //}

            return Initialize<GraphLookupPartSettingsViewModel>("GraphLookupPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<GraphLookupPartSettings>();

                //model.Pattern = settings.Pattern;
                model.GraphLookupPartSettings = settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            //todo: is check strictly necessary? if so can't return null, can't change signature. throw?
            //if (!string.Equals(nameof(GraphLookupPart), contentTypePartDefinition.PartDefinition.Name))
            //{
            //    return default;
            //}

            var model = new GraphLookupPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.NodeLabel))
            {
                context.Builder.WithSettings(new GraphLookupPartSettings { NodeLabel = model.NodeLabel });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
