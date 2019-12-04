using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.Editor.Module.Settings
{
    public class UriFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<UriField>
    {
#pragma warning disable S927 // parameter names should match base declaration and other partial definitions
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<UriFieldSettings>("UriFieldSettings_Edit", model => partFieldDefinition.PopulateSettings(model))
                .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
#pragma warning restore S927 // parameter names should match base declaration and other partial definitions
        {
            var model = new UriFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model);

            return Edit(partFieldDefinition);
        }
    }
}
