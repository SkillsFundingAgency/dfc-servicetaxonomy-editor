using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace DFC.ServiceTaxonomy.ContentPickerPreview
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition("ContentPickerPreviewPart", builder => builder
                .Attachable()
                .WithDescription("Provides a content picker with a preview of the picked content.")
            );

            return 1;
        }
    }
}
