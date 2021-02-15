using DFC.ServiceTaxonomy.ContentApproval.Indexes;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using YesSql.Sql;

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

        public int UpdateFrom1()
        {
            SchemaBuilder.CreateMapIndexTable<ContentApprovalPartIndex>(table => table
                .Column<string>(nameof(ContentApprovalPartIndex.ApprovalStatus))
            );

            SchemaBuilder.AlterTable(nameof(ContentApprovalPartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(ContentApprovalPartIndex)}_{nameof(ContentApprovalPartIndex.ApprovalStatus)}",
                    nameof(ContentApprovalPartIndex.ApprovalStatus))
            );
            return 2;
        }
    }
}
