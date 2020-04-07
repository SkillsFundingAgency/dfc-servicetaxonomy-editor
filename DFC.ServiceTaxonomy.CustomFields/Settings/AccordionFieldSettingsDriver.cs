using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CustomFields.Fields;
using DFC.ServiceTaxonomy.CustomFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.CustomFields.Settings
{
    public class AccordionFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<AccordionField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<EditAccordionFieldSettingsViewModel>("AccordionFieldSettings_Edit", model =>
            {
                var settings = partFieldDefinition.GetSettings<AccordionFieldSettings>();

                model.HeaderText = settings.HeaderText;
            })
            .Location("Content");

        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new EditAccordionFieldSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                context.Builder.WithSettings(new AccordionFieldSettings
                {
                    HeaderText = model.HeaderText
                });
            }

            return Edit(partFieldDefinition);
        }
    }
}
