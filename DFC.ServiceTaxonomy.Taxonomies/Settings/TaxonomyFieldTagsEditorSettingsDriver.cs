using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using DFC.ServiceTaxonomy.Taxonomies.Fields;

namespace DFC.ServiceTaxonomy.Taxonomies.Settings
{
    public class TaxonomyFieldTagsEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver<TaxonomyField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<TaxonomyFieldTagsEditorSettings>("TaxonomyFieldTagsEditorSettings_Edit", model => partFieldDefinition.PopulateSettings(model))
                .Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (partFieldDefinition.Editor() == "Tags")
            {
                var model = new TaxonomyFieldTagsEditorSettings();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                context.Builder.WithSettings(model);
            }

            return Edit(partFieldDefinition);
        }
    }
}
