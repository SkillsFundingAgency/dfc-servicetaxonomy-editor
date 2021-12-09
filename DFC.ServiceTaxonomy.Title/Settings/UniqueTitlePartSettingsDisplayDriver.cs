using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Title.Models;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.Title.Settings
{
    public class UniqueTitlePartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly IOptionsMonitor<UniqueTitlePartSettingsConfiguration> _uniqueTitlePartSettingsConfiguration;

        public UniqueTitlePartSettingsDisplayDriver(IOptionsMonitor<UniqueTitlePartSettingsConfiguration> uniqueTitlePartSettingsConfiguration)
        {
            _uniqueTitlePartSettingsConfiguration = uniqueTitlePartSettingsConfiguration;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition)
        {
            if (!string.Equals(nameof(UniqueTitlePart), contentTypePartDefinition.PartDefinition.Name))
            {
                return default!;
            }

            return Initialize<UniqueTitlePartSettingsViewModel>("UniqueTitlePartSettings_Edit", model =>
            {
                UniqueTitlePartSettings uniqueTitlePartSettings = contentTypePartDefinition.GetSettings<UniqueTitlePartSettings>();

                model.Hint = uniqueTitlePartSettings.Hint;

                BuildUniqueTitlePartSettingsList(model);
            })
                .Location("Content");
        }

        private void BuildUniqueTitlePartSettingsList(UniqueTitlePartSettingsViewModel model)
        {

            foreach (var item in _uniqueTitlePartSettingsConfiguration.CurrentValue.Settings)
            {
                model.AllSettings.Add(item);
            }


        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!string.Equals(nameof(UniqueTitlePart), contentTypePartDefinition.PartDefinition.Name))
            {
                return default!;
            }

            var model = new UniqueTitlePartSettingsViewModel();


            if (await context.Updater.TryUpdateModelAsync(model, Prefix,
                m => m.Hint))
            {
                context.Builder.WithSettings(new UniqueTitlePartSettings
                {
                    Hint = model.Hint
                });
            }

            return Edit(contentTypePartDefinition);
        }


    }
}
