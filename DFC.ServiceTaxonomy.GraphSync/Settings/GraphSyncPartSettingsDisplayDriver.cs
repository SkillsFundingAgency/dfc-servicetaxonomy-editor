using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using DFC.ServiceTaxonomy.GraphSync.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Settings
{
    public class GraphSyncPartSettingsDisplayDriver : ContentPartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentPartDefinition contentPartDefinition)
        {
            if (!String.Equals(nameof(GraphSyncPart), contentPartDefinition.Name))
            {
                return null;
            }

            return Initialize<GraphSyncPartSettingsViewModel>("GraphSyncPartSettings_Edit", model =>
            {
                var settings = contentPartDefinition.GetSettings<GraphSyncPartSettings>();

                model.MySetting = settings.MySetting;
                model.GraphSyncPartSettings = settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartDefinition contentPartDefinition, UpdatePartEditorContext context)
        {
            if (!String.Equals(nameof(GraphSyncPart), contentPartDefinition.Name))
            {
                return null;
            }

            var model = new GraphSyncPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.MySetting))
            {
                context.Builder.WithSettings(new GraphSyncPartSettings { MySetting = model.MySetting });
            }

            return Edit(contentPartDefinition);
        }
    }
}
