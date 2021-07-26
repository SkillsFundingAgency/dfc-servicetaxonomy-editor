using OrchardCore.ContentFields.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using YesSql;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.Services
{
    public class BannerContentPickerResultProvider : DefaultContentPickerResultProvider, IContentPickerResultProvider
    {
        public BannerContentPickerResultProvider(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager, ISession session)
            : base(contentManager, contentDefinitionManager, session)
        {
        }

        public new string Name => "Banner";
    }
}
