using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Fields;
using DFC.ServiceTaxonomy.Editor.Module.Settings;
using DFC.ServiceTaxonomy.Editor.Module.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.Editor.Module.Drivers
{
    public class UriFieldDisplayDriver : ContentFieldDisplayDriver<UriField>
    {
        public override IDisplayResult Display(UriField field, BuildFieldDisplayContext fieldDisplayContext)
        {
            return Initialize<DisplayUriFieldViewModel>(GetDisplayShapeType(fieldDisplayContext), model =>
                {
                    model.Field = field;
                    model.Part = fieldDisplayContext.ContentPart;
                    model.PartFieldDefinition = fieldDisplayContext.PartFieldDefinition;
                })
                .Location("Content")
                .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(UriField field, BuildFieldEditorContext context)
        {
            return Initialize<EditUriFieldViewModel>(GetEditorShapeType(context), model =>
            {
                // why is the prefix options empty?

                model.Text = field.Text ?? $"{context.PartFieldDefinition.GetSettings<UriFieldSettings>().NamespacePrefix}{context.TypePartDefinition.Name.ToLowerInvariant()}/{Guid.NewGuid():D}";
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(UriField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            await updater.TryUpdateModelAsync(field, Prefix, f => f.Text);

            return Edit(field, context);
        }
    }
}
