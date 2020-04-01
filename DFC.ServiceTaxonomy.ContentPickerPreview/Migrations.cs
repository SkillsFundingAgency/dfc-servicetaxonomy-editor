using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using YesSql;

namespace DFC.ServiceTaxonomy.ContentPickerPreview
{
    public class Migrations : DataMigration
    {
        private IContentDefinitionManager _contentDefinitionManager;
        private readonly ISession _session;
        private readonly ILogger<Migrations> _logger;

        public Migrations(
            IContentDefinitionManager contentDefinitionManager,
            ISession session,
            ILogger<Migrations> logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _session = session;
            _logger = logger;
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
