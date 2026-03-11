using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Taxonomies.Fields;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.Taxonomies.Settings
{
    public class TaxonomyFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<TaxonomyField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
        {
            return Initialize<TaxonomyFieldSettings>("TaxonomyFieldSettings_Edit", model => {
                var settings = partFieldDefinition.GetSettings<TaxonomyFieldSettings>();
                model.Hint = settings.Hint;

            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new TaxonomyFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model);

            return Edit(partFieldDefinition, context);
        }
    }
}
