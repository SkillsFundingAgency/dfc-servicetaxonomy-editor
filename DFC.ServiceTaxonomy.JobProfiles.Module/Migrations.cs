using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.Indexes;
using YesSql.Sql;
using DFC.ServiceTaxonomy.JobProfiles.Module.Indexes;

namespace DFC.ServiceTaxonomy.JobProfiles.Module
{
    [Feature("DFC.ServiceTaxonomy.JobProfiles")]
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(JobProfile), part => part
                .Attachable()
                .WithDescription("Creates JobProfile related metadata record. ")
            );

            SchemaBuilder.CreateMapIndexTable<JobProfileIndex>(table => table
                .Column<string>(nameof(JobProfileIndex.ContentItemId))
                .Column<string>(nameof(JobProfileIndex.GraphSyncPartId))
                .Column<string>(nameof(JobProfileIndex.DynamicTitlePrefix))
                .Column<string>(nameof(JobProfileIndex.JobProfileSpecialism))
                .Column<string>(nameof(JobProfileIndex.JobProfileCategory))
                .Column<string>(nameof(JobProfileIndex.RelatedCareerProfiles))
                .Column<string>(nameof(JobProfileIndex.SOCCode))
                .Column<string>(nameof(JobProfileIndex.HiddenAlternativeTitle))
                .Column<string>(nameof(JobProfileIndex.WorkingHoursDetail))
                .Column<string>(nameof(JobProfileIndex.WorkingPatterns))
                .Column<string>(nameof(JobProfileIndex.WorkingPatternDetail))
                .Column<string>(nameof(JobProfileIndex.UniversityEntryRequirements))
                .Column<string>(nameof(JobProfileIndex.UniversityRequirements))
                .Column<string>(nameof(JobProfileIndex.UniversityLinks))
                .Column<string>(nameof(JobProfileIndex.CollegeEntryRequirements))
                .Column<string>(nameof(JobProfileIndex.CollegeRequirements))
                .Column<string>(nameof(JobProfileIndex.CollegeLink))
                .Column<string>(nameof(JobProfileIndex.ApprenticeshipEntryRequirements))
                .Column<string>(nameof(JobProfileIndex.ApprenticeshipRequirements))
                .Column<string>(nameof(JobProfileIndex.ApprenticeshipLink))
                .Column<string>(nameof(JobProfileIndex.Registration))
                .Column<string>(nameof(JobProfileIndex.DigitalSkills))
                .Column<string>(nameof(JobProfileIndex.RelatedSkills), column => column.WithLength(1024))
                .Column<string>(nameof(JobProfileIndex.Location))
                .Column<string>(nameof(JobProfileIndex.Environment))
                .Column<string>(nameof(JobProfileIndex.Uniform))
                .Column<string>(nameof(JobProfileIndex.JobProfileTitle))
                .Column<string>(nameof(JobProfileIndex.Restriction)));


            SchemaBuilder.AlterIndexTable<JobProfileIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}",
                    "DocumentId",
                    nameof(JobProfileIndex.ContentItemId),
                    nameof(JobProfileIndex.GraphSyncPartId)));

            return 6;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.AlterIndexTable<JobProfileIndex>(table => table.AddColumn<string>(nameof(JobProfileIndex.JobProfileTitle)));

            SchemaBuilder.AlterIndexTable<JobProfileIndex>(table => table
                .DropIndex($"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}"));

            SchemaBuilder.AlterIndexTable<JobProfileIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}",
                    "DocumentId",
                    nameof(JobProfileIndex.ContentItemId),
                    nameof(JobProfileIndex.GraphSyncPartId)));

            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.AlterIndexTable<JobProfileIndex>(table => table.AddColumn<string>(nameof(JobProfileIndex.Restriction)));

            SchemaBuilder.AlterIndexTable<JobProfileIndex>(table => table
                .DropIndex($"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}"));

            SchemaBuilder.AlterIndexTable<JobProfileIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}",
                    "DocumentId",
                    nameof(JobProfileIndex.ContentItemId),
                    nameof(JobProfileIndex.GraphSyncPartId)));

            return 3;
        }

        public int UpdateFrom3()
        {
            SchemaBuilder.AlterIndexTable<JobProfileIndex>(table => table.DropColumn("Digitalskills"));

            SchemaBuilder.AlterIndexTable<JobProfileIndex>(table => table.AddColumn<string>(nameof(JobProfileIndex.DigitalSkills)));

            SchemaBuilder.AlterIndexTable<JobProfileIndex>(table => table
                .DropIndex($"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}"));

            SchemaBuilder.AlterIndexTable<JobProfileIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}",
                    "DocumentId",
                    nameof(JobProfileIndex.ContentItemId),
                    nameof(JobProfileIndex.GraphSyncPartId)));

            return 4;
        }

        public int UpdateFrom4()
        {
            SchemaBuilder.AlterIndexTable<JobProfileIndex>(table => table.AddColumn<string>(nameof(JobProfileIndex.RelatedSkills)));

            SchemaBuilder.AlterIndexTable<JobProfileIndex>(table => table
                .DropIndex($"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}"));

            SchemaBuilder.AlterIndexTable<JobProfileIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}",
                    "DocumentId",
                    nameof(JobProfileIndex.ContentItemId),
                    nameof(JobProfileIndex.GraphSyncPartId)));

            return 5;
        }

        public int UpdateFrom5()
        {
            SchemaBuilder.AlterIndexTable<JobProfileIndex>(table => table.DropColumn(nameof(JobProfileIndex.RelatedSkills)));
            SchemaBuilder.AlterIndexTable<JobProfileIndex>(table => table.AddColumn<string>(nameof(JobProfileIndex.RelatedSkills), column => column.WithLength(1024)));

            SchemaBuilder.AlterIndexTable<JobProfileIndex>(table => table
                .DropIndex($"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}"));

            SchemaBuilder.AlterIndexTable<JobProfileIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}",
                    "DocumentId",
                    nameof(JobProfileIndex.ContentItemId),
                    nameof(JobProfileIndex.GraphSyncPartId)));

            return 6;
        }



    }
}
