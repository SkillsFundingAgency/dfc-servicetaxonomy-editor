using DFC.ServiceTaxonomy.ContentApproval.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace DFC.ServiceTaxonomy.ContentApproval.Migrations
{
    public class ContentApprovalPartMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentApprovalPartMigrations(IContentDefinitionManager contentDefinitionManager) =>
            _contentDefinitionManager = contentDefinitionManager;

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(ContentApprovalPart), part => part
                .Attachable()
                .WithDescription("Adds publishing status workflow properties to content items.")
            );

            return 1;
        }
    }
}
