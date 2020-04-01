using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentPickerPreview.Models;
using DFC.ServiceTaxonomy.ContentPickerPreview.ViewModels;
using Fluid;
//using Fluid;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Liquid;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.Handlers
{
    public class ContentPickerPreviewPartHandler : ContentPartHandler<ContentPickerPreviewPart>
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentPickerPreviewPartHandler(
            ILiquidTemplateManager liquidTemplateManager,
            IContentDefinitionManager contentDefinitionManager)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override Task UpdatedAsync(UpdateContentContext context, ContentPickerPreviewPart part)
        {
            var settings = GetSettings(part);
            // Do not compute the title if the user can modify it and the text is already set.
            // if (settings.Options == TitlePartOptions.Editable && !String.IsNullOrWhiteSpace(part.ContentItem.DisplayText))
            // {
            //     return;
            // }

            // if (!String.IsNullOrEmpty(settings.Pattern))
            // {
            //     var model = new ContentPickerPreviewPartViewModel
            //     {
            //         Title = part.Title,
            //         TitlePart = part,
            //         ContentItem = part.ContentItem
            //     };
            //
            //     var title = await _liquidTemplateManager.RenderAsync(settings.Pattern, NullEncoder.Default, model,
            //         scope => scope.SetValue("ContentItem", model.ContentItem));
            //
            //     title = title.Replace("\r", String.Empty).Replace("\n", String.Empty);
            //
            //     part.Title = title;
            //     part.ContentItem.DisplayText = title;
            //     part.Apply();
            // }
            return Task.CompletedTask;
        }

        private ContentPickerPreviewPartSettings GetSettings(ContentPickerPreviewPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.PartDefinition.Name, nameof(ContentPickerPreviewPart)));
            return contentTypePartDefinition.GetSettings<ContentPickerPreviewPartSettings>();
        }
    }
}
