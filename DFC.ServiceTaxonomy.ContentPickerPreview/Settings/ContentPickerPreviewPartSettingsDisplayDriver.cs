using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentPickerPreview.Models;
using DFC.ServiceTaxonomy.ContentPickerPreview.ViewModels;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.Settings
{
    public class ContentPickerPreviewPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly ILiquidTemplateManager _templateManager;
        private readonly IStringLocalizer<ContentPickerPreviewPartSettingsDisplayDriver> S;

        public ContentPickerPreviewPartSettingsDisplayDriver(ILiquidTemplateManager templateManager, IStringLocalizer<ContentPickerPreviewPartSettingsDisplayDriver> localizer)
        {
            _templateManager = templateManager;
            S = localizer;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!string.Equals(nameof(ContentPickerPreviewPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return default!;
            }

            return Initialize<ContentPickerPreviewPartSettingsViewModel>("ContentPickerPreviewPartSettings_Edit", model =>
            {
                // var settings = contentTypePartDefinition.GetSettings<ContentPickerPreviewPartSettings>();
                //
                // model.Options = settings.Options;
                // model.Pattern = settings.Pattern;
                // model.ContentPickerPreviewPartSettings = settings;
            }).Location("Content");
        }

        public override Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!string.Equals(nameof(ContentPickerPreviewPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return default!;
            }

            // var model = new ContentPickerPreviewPartSettingsViewModel();
            //
            // await context.Updater.TryUpdateModelAsync(model, Prefix,
            //     m => m.Pattern,
            //     m => m.Options);
            //
            // if (!string.IsNullOrEmpty(model.Pattern) && !_templateManager.Validate(model.Pattern, out var errors))
            // {
            //     context.Updater.ModelState.AddModelError(nameof(model.Pattern), S["Pattern doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)]);
            // }
            // else
            // {
            //     context.Builder.WithSettings(new ContentPickerPreviewPartSettings { Pattern = model.Pattern, Options = model.Options });
            // }

            return Task.FromResult(Edit(contentTypePartDefinition, context.Updater));
        }
    }
}
