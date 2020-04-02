using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentPickerPreview.Models;
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
                var settings = contentTypePartDefinition.GetSettings<ContentPickerPreviewPartSettings>();

                model.Hint = settings.Hint;
                model.Required = settings.Required;
                model.EditButton = settings.EditButton;
                model.DisplayedContentTypes = settings.DisplayedContentTypes;
                model.DisplayAllContentTypes = settings.DisplayAllContentTypes;

            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!string.Equals(nameof(ContentPickerPreviewPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return default!;
            }

            var model = new ContentPickerPreviewPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix,
                m => m.Hint,
                m => m.Required,
                m => m.EditButton,
                m => m.DisplayedContentTypes,
                m => m.DisplayAllContentTypes))
            {
                if (model.DisplayAllContentTypes)
                {
                    model.DisplayedContentTypes = Array.Empty<String>();
                }

                context.Builder.WithSettings(new ContentPickerPreviewPartSettings
                {
                    Hint = model.Hint,
                    Required = model.Required,
                    EditButton = model.EditButton,
                    DisplayedContentTypes = model.DisplayedContentTypes,
                    DisplayAllContentTypes = model.DisplayAllContentTypes
                });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
