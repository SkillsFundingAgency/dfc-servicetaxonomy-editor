using OrchardCore.ContentFields.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Liquid;
using YesSql;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.Services
{
    public class EditLinkContentPickerResultProvider : DefaultContentPickerResultProvider, IContentPickerResultProvider
    {
        public EditLinkContentPickerResultProvider(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager, ISession session, ILiquidTemplateManager templateManager)
           : base(contentManager, contentDefinitionManager, session, templateManager)
        {
        }

        public new string Name => "EditLink";
    }
}
