using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CustomFields.Fields;
using DFC.ServiceTaxonomy.CustomFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.CustomFields.Drivers
{
    public class AccordionFieldDisplayDriver : ContentFieldDisplayDriver<AccordionField>
    {
        public override IDisplayResult Display(AccordionField field, BuildFieldDisplayContext context)
        {
            return Initialize<EmptyViewModel>(GetDisplayShapeType(context), model =>
            {})
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(AccordionField field, BuildFieldEditorContext context)
        {
            return Initialize<EditAccordionFieldViewModel>(GetEditorShapeType(context), model =>
            {
                model.PartName = context.PartFieldDefinition.PartDefinition.Name;
                model.HeaderText = context.PartFieldDefinition.DisplayName();
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(AccordionField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            return await EditAsync(field, context);
        }
    }
}
