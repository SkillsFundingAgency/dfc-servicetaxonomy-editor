using OrchardCore.ContentFields.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using YesSql;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.Services
{
    public class EditLinkContentPickerResultProvider : DefaultContentPickerResultProvider, IContentPickerResultProvider
    {
        public EditLinkContentPickerResultProvider(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager, ISession session)
           : base(contentManager, contentDefinitionManager, session)
        {
        }

        public new string Name => "EditLink";
    }
}
