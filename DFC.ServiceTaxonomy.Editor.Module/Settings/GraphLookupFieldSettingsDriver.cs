using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.Editor.Module.Settings
{
#pragma warning disable S927 // parameter names should match base declaration and other partial definitions
    public class GraphLookupFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<GraphLookupField>
    {
        public GraphLookupFieldSettingsDriver()
        {
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<GraphLookupFieldSettings>("GraphLookupFieldSettings_Edit", model =>
            {
                partFieldDefinition.PopulateSettings(model);
            })
            .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new GraphLookupFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model);

            return Edit(partFieldDefinition);
        }
#pragma warning restore S927 // parameter names should match base declaration and other partial definitions
    }
}
