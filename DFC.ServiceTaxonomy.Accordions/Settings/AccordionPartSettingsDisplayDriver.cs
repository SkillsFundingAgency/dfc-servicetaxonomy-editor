using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using DFC.ServiceTaxonomy.Accordions.Models;

namespace DFC.ServiceTaxonomy.Accordions.Settings
{
    public class AccordionPartSettingsDisplayDriver : ContentPartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentPartDefinition contentPartDefinition)
        {
            if (!String.Equals(nameof(AccordionPart), contentPartDefinition.Name))
            {
                return null;
            }

            return Initialize<AccordionPartSettingsViewModel>("AccordionPartSettings_Edit", model =>
            {
                var settings = contentPartDefinition.GetSettings<AccordionPartSettings>();

                model.AccordionPartSettings = settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartDefinition contentPartDefinition, UpdatePartEditorContext context)
        {
            if (!String.Equals(nameof(AccordionPart), contentPartDefinition.Name))
            {
                return null;
            }

            var model = new AccordionPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                context.Builder.WithSettings(new AccordionPartSettings());
            }

            return Edit(contentPartDefinition);
        }
    }
}
