using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.Editor.Module.Drivers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1185:Overriding members should do more than simply call the same member in the base class", Justification = "temporary")]
    public class UriFieldDisplayDriver : TextFieldDisplayDriver
    {
        public UriFieldDisplayDriver(IStringLocalizer<TextFieldDisplayDriver> localizer) : base(localizer)
        {
        }

        public override IDisplayResult Display(TextField field, BuildFieldDisplayContext context)
        {
            return base.Display(field, context);
        }

        public override IDisplayResult Edit(TextField field, BuildFieldEditorContext context)
        {
            return base.Edit(field, context);
        }

        public override Task<IDisplayResult> UpdateAsync(TextField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            return base.UpdateAsync(field, updater, context);
        }
    }
}
