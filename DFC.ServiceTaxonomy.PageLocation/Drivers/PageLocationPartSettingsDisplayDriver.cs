using System.Threading.Tasks;
using DFC.ServiceTaxonomy.PageLocation.Models;
using DFC.ServiceTaxonomy.PageLocation.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.PageLocation.Drivers
{
    public class PageLocationPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition)
        {
            if (!string.Equals(nameof(PageLocationPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return default!;
            }

            return Initialize<PageLocationPartSettingsViewModel>("PageLocationPartSettings_Edit", model =>
            {
                PageLocationPartSettings settings = contentTypePartDefinition.GetSettings<PageLocationPartSettings>();
                model.DisplayRedirectLocationsAndDefaultPageForLocation = settings.DisplayRedirectLocationsAndDefaultPageForLocation;
            })
            .Location("Content");
        }

        public override async Task<IDisplayResult?> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!string.Equals(nameof(PageLocationPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return null;
            }

            var model = new PageLocationPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix,
                m => m.DisplayRedirectLocationsAndDefaultPageForLocation);

            context.Builder.WithSettings(new PageLocationPartSettings { DisplayRedirectLocationsAndDefaultPageForLocation = model.DisplayRedirectLocationsAndDefaultPageForLocation });

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
