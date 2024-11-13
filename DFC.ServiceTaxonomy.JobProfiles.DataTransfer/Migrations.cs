using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Indexes;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.Indexes;

using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;

using YesSql.Sql;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer
{
    [Feature("DFC.ServiceTaxonomy.JobProfiles")]
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager) =>
            _contentDefinitionManager = contentDefinitionManager;

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinitionAsync(nameof(JobProfile), part => part
                .Attachable()
                .WithDescription("Creates JobProfile related metadata record. ")
            );

            SchemaBuilder.CreateMapIndexTableAsync<JobProfileIndex>(table => table
                .Column<string>(nameof(JobProfileIndex.ContentItemId))
                .Column<string>(nameof(JobProfileIndex.GraphSyncPartId))
                .Column<string>(nameof(JobProfileIndex.DynamicTitlePrefix))
                .Column<string>(nameof(JobProfileIndex.JobProfileSpecialism))
                .Column<string>(nameof(JobProfileIndex.JobProfileCategory))
                .Column<string>(nameof(JobProfileIndex.RelatedCareerProfiles))
                .Column<string>(nameof(JobProfileIndex.SOCCode))
                .Column<string>(nameof(JobProfileIndex.HiddenAlternativeTitle), column => column.WithLength(600))
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
                .Column<string>(nameof(JobProfileIndex.RelatedSkills), column => column.WithLength(600))
                .Column<string>(nameof(JobProfileIndex.Location))
                .Column<string>(nameof(JobProfileIndex.Environment))
                .Column<string>(nameof(JobProfileIndex.Uniform))
                .Column<string>(nameof(JobProfileIndex.JobProfileTitle))
                .Column<string>(nameof(JobProfileIndex.Restriction)));


            SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}",
                    "DocumentId",
                    nameof(JobProfileIndex.ContentItemId),
                    nameof(JobProfileIndex.GraphSyncPartId)));

            return 7;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table.AddColumn<string>(nameof(JobProfileIndex.JobProfileTitle)));

            SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table
                .DropIndex($"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}"));

            SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}",
                    "DocumentId",
                    nameof(JobProfileIndex.ContentItemId),
                    nameof(JobProfileIndex.GraphSyncPartId)));

            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table.AddColumn<string>(nameof(JobProfileIndex.Restriction)));

            SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table
                .DropIndex($"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}"));

            SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}",
                    "DocumentId",
                    nameof(JobProfileIndex.ContentItemId),
                    nameof(JobProfileIndex.GraphSyncPartId)));

            return 3;
        }

        public int UpdateFrom3()
        {
            SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table.DropColumn("Digitalskills"));

            SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table.AddColumn<string>(nameof(JobProfileIndex.DigitalSkills)));

            SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table
                .DropIndex($"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}"));

            SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}",
                    "DocumentId",
                    nameof(JobProfileIndex.ContentItemId),
                    nameof(JobProfileIndex.GraphSyncPartId)));

            return 4;
        }

        public int UpdateFrom4()
        {
            SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table.AddColumn<string>(nameof(JobProfileIndex.RelatedSkills)));

            SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table
                .DropIndex($"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}"));

            SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}",
                    "DocumentId",
                    nameof(JobProfileIndex.ContentItemId),
                    nameof(JobProfileIndex.GraphSyncPartId)));

            return 5;
        }

        public int UpdateFrom5()
        {
            try
            {
                SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table.DropColumn(nameof(JobProfileIndex.RelatedSkills)));
                SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(
                    table => table.AddColumn<string>(nameof(JobProfileIndex.RelatedSkills),
                    column => column.WithLength(1024)));

                SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table
                    .DropIndex($"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}"));

                SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table
                    .CreateIndex(
                        $"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}",
                        "DocumentId",
                        nameof(JobProfileIndex.ContentItemId),
                        nameof(JobProfileIndex.GraphSyncPartId)));
            }
            catch
            {
                // SQLLite will throw an error here so we ignore it as it is not concerned with column length constraints
            }

            return 6;
        }

        public int UpdateFrom6()
        {
            try
            {
                SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table.DropColumn(nameof(JobProfileIndex.RelatedSkills)));
                SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(
                    table => table.AddColumn<string>(nameof(JobProfileIndex.RelatedSkills),
                    column => column.WithLength(600)));

                SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table.DropColumn(nameof(JobProfileIndex.HiddenAlternativeTitle)));
                SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(
                    table => table.AddColumn<string>(nameof(JobProfileIndex.HiddenAlternativeTitle),
                    column => column.WithLength(300)));

                SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table
                    .DropIndex($"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}"));

                SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table
                    .CreateIndex(
                        $"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}",
                        "DocumentId",
                        nameof(JobProfileIndex.ContentItemId),
                        nameof(JobProfileIndex.GraphSyncPartId)));
            }
            catch
            {
                // SQLLite will throw an error here so we ignore it as it is not concerned with column length constraints
            }

            return 7;
        }

        public int UpdateFrom7()
        {
            try
            {
                SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table.DropColumn(nameof(JobProfileIndex.HiddenAlternativeTitle)));
                SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(
                    table => table.AddColumn<string>(nameof(JobProfileIndex.HiddenAlternativeTitle),
                    column => column.WithLength(600)));

                SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table
                    .DropIndex($"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}"));

                SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(table => table
                    .CreateIndex(
                        $"IDX_{nameof(JobProfileIndex)}_{nameof(JobProfileIndex.ContentItemId)}",
                        "DocumentId",
                        nameof(JobProfileIndex.ContentItemId),
                        nameof(JobProfileIndex.GraphSyncPartId)));
            }
            catch
            {
                // SQLLite will throw an error here so we ignore it as it is not concerned with column length constraints
            }

            return 8;
        }

        public int UpdateFrom8()
        {
            try
            {
                SchemaBuilder.AlterIndexTableAsync<JobProfileIndex>(
                    table => table.AddColumn<string>(nameof(JobProfileIndex.RealStory),
                    column => column.WithLength(600)));
            }
            catch
            {
                // SQLLite will throw an error here so we ignore it as it is not concerned with column length constraints
            }

            return 9;
        }

        public int UpdateFrom9()
        {
            _contentDefinitionManager.AlterPartDefinitionAsync(nameof(JobProfileSimplificationPart), builder => builder
               .Attachable()
               .WithDescription("Provides a JobProfileSimplificationPart for your content item."));

            return 10;
        }
    }
}
