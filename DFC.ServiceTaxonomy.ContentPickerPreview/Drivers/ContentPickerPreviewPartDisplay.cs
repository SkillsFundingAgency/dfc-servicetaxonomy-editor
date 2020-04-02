using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentPickerPreview.Models;
using DFC.ServiceTaxonomy.ContentPickerPreview.ViewModels;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.Drivers
{
    //probably need DefaultContentPickerResultProvider
    // could we reuse the existing ContentPicker for the graph lookup and use a different IContentPickerResultProvider?
    public class ContentPickerPreviewPartDisplay : ContentPartDisplayDriver<ContentPickerPreviewPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IStringLocalizer S;

        public ContentPickerPreviewPartDisplay(IContentDefinitionManager contentDefinitionManager,
            IStringLocalizer<ContentPickerPreviewPartDisplay> localizer)
        {
            S = localizer;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(ContentPickerPreviewPart contentPickerPreviewPart,
            BuildPartDisplayContext context)
        {
            return Initialize<ContentPickerPreviewPartViewModel>(GetDisplayShapeType(context), model =>
                {
                    // model.Title = titlePart.ContentItem.DisplayText;
                    // model.TitlePart = titlePart;
                })
                .Location("Detail", "Header:5")
                .Location("Summary", "Header:5");
        }

        public override IDisplayResult Edit(ContentPickerPreviewPart contentPickerPreviewPart,
            BuildPartEditorContext context)
        {
            return Initialize<ContentPickerPreviewPartViewModel>(GetEditorShapeType(context), model =>
            {
                // model.Title = titlePart.ContentItem.DisplayText;
                // model.TitlePart = titlePart;
                //model.Settings = context.TypePartDefinition.GetSettings<ContentPickerPreviewPartSettings>();
                model.Settings = GetPartSettings(contentPickerPreviewPart);
            });
        }

        public override Task<IDisplayResult> UpdateAsync(ContentPickerPreviewPart model, IUpdateModel updater,
            UpdatePartEditorContext context)
        {
            // if (await updater.TryUpdateModelAsync(model, Prefix, t => t.Title))
            // {
                // var settings = context.TypePartDefinition.GetSettings<ContentPickerPreviewPartSettings>();
                // if (settings.Options == ContentPickerPreviewPartOptions.EditableRequired && String.IsNullOrWhiteSpace(model.Title))
                // {
                //     updater.ModelState.AddModelError(Prefix, S["A value is required for Title."]);
                // }
            // }

            // model.ContentItem.DisplayText = model.Title;

            return Task.FromResult(Edit(model, context));
        }

        // extension method?
        private ContentPickerPreviewPartSettings GetPartSettings(ContentPickerPreviewPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(ContentPickerPreviewPart));
            return contentTypePartDefinition.GetSettings<ContentPickerPreviewPartSettings>();
        }
    }
}
