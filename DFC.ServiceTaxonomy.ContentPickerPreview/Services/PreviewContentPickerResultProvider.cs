using OrchardCore.ContentFields.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using YesSql;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.Services
{
    public class PreviewContentPickerResultProvider : DefaultContentPickerResultProvider, IContentPickerResultProvider
    {
        public PreviewContentPickerResultProvider(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager, ISession session)
            : base(contentManager, contentDefinitionManager, session)
        {
        }

        public new string Name => "Preview";
    }
}
