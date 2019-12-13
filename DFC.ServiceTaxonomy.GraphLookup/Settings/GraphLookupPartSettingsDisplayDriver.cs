using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using DFC.ServiceTaxonomy.GraphLookup.Models;

namespace DFC.ServiceTaxonomy.GraphLookup.Settings
{
    public class GraphLookupPartSettingsDisplayDriver : ContentPartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentPartDefinition contentPartDefinition)
        {
            if (!String.Equals(nameof(GraphLookupPart), contentPartDefinition.Name))
            {
                return null;
            }

            return Initialize<GraphLookupPartSettingsViewModel>("GraphLookupPartSettings_Edit", model =>
            {
                var settings = contentPartDefinition.GetSettings<GraphLookupPartSettings>();

                model.MySetting = settings.MySetting;
                model.GraphLookupPartSettings = settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartDefinition contentPartDefinition, UpdatePartEditorContext context)
        {
            if (!String.Equals(nameof(GraphLookupPart), contentPartDefinition.Name))
            {
                return null;
            }

            var model = new GraphLookupPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.MySetting))
            {
                context.Builder.WithSettings(new GraphLookupPartSettings { MySetting = model.MySetting });
            }

            return Edit(contentPartDefinition);
        }
    }
}
