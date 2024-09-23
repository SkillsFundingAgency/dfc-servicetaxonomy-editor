using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Recipes.Services;

namespace DFC.ServiceTaxonomy.ExploreCareersMigration.Migrations
{
    public class ExploreCareersMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IRecipeMigrator _recipeMigrator;
        private readonly ILogger<ExploreCareersMigrations> _logger;

        public ExploreCareersMigrations(IContentDefinitionManager contentDefinitionManager ,IRecipeMigrator recipeMigrator, ILogger<ExploreCareersMigrations> logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _recipeMigrator = recipeMigrator;
            _logger = logger;
        }

        public async Task<int> CreateAsync()
        {
            try
            {
                _logger.LogInformation($"Starting step 1 of DFC.ServiceTaxonomy.ExploreCareersMigration - Adding Job Profile + Job Profile Sector content type");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/explore-careers-content-types-migration-step-01.recipe.json", this);
                _logger.LogInformation($"Completed step 1 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 1 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }

            return 1;
        }

        public async Task<int> UpdateFrom1Async()
        {
            try
            {
                _logger.LogInformation($"Starting step 2 of DFC.ServiceTaxonomy.ExploreCareersMigration - Adding Job Profile Sector content items");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/explore-careers-content-types-migration-step-02.recipe.json", this);
                _logger.LogInformation($"Completed step 2 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 2 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }

            return 2;
        }

        public async Task<int> UpdateFrom2Async()
        {
            try
            {
                _logger.LogInformation($"Starting step 3 of DFC.ServiceTaxonomy.ExploreCareersMigration - Adding placements for updated Job Profile + Job Profile Sector");
                await _recipeMigrator.ExecuteAsync("MiMigrationRecipes/explore-careers-content-types-migration-step-03.recipe.json", this);
                _logger.LogInformation($"Completed step 3 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 3 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }

            return 3;
        }

        // This is a specific step to sanitise the SectorLandingPage type from the various testing and iterations the EC team has performed on LAB and other testing environments
        // If the parts/fields do not exist, the step will still progress.
        public int UpdateFrom3()
        {
            try
            {
                _logger.LogInformation($"Starting step 4 of DFC.ServiceTaxonomy.ExploreCareersMigration - Sanitising SectorLandingPage of test iterations");

                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition("SectorLandingPage");

                if (contentTypeDefinition != null)
                {
                    // Remove specified parts
                    var partsToRemove = new[] { "ContentApprovalPart", "SitemapPart", "AuditTrailPart" };
                    foreach (var partName in partsToRemove)
                    {
                        var partToRemove = contentTypeDefinition.Parts.FirstOrDefault(p => p.Name == partName);
                        if (partToRemove != null)
                        {
                            _contentDefinitionManager.AlterTypeDefinition("SectorLandingPage", type => type.RemovePart(partName));
                            _logger.LogInformation($"Part '{partName}' removed from content type 'SectorLandingPage'");
                        }
                        else
                        {
                            _logger.LogInformation($"Part '{partName}' not found in content type 'SectorLandingPage' - continuing");
                        }
                    }

                    // Remove all fields from the "SectorLandingPage" part
                    var sectorLandingPagePart = contentTypeDefinition.Parts.FirstOrDefault(p => p.Name == "SectorLandingPage");
                    if (sectorLandingPagePart != null)
                    {
                        foreach (var field in sectorLandingPagePart.PartDefinition.Fields.ToList())
                        {
                            _contentDefinitionManager.AlterPartDefinition(sectorLandingPagePart.PartDefinition.Name, part =>
                                part.RemoveField(field.Name)
                            );
                            _logger.LogInformation($"Field '{field.Name}' removed from part 'SectorLandingPage'");
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Part 'SectorLandingPage' not found in content type 'SectorLandingPage' - continuing");
                    }
                }

                _logger.LogInformation($"Completed step 4 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during step 4 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }

            return 4;
        }




        public async Task<int> UpdateFrom4Async()
        {
            try
            {
                _logger.LogInformation($"Starting step 5 of DFC.ServiceTaxonomy.ExploreCareersMigration - Adding Sector Landing Page content type");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/explore-careers-content-types-migration-step-05.recipe.json", this);
                _logger.LogInformation($"Completed step 5 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 5 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }

            return 5;
        }

        public async Task<int> UpdateFrom5Async()
        {
            try
            {
                _logger.LogInformation($"Starting step 6 of DFC.ServiceTaxonomy.ExploreCareersMigration - Adding placements for updated Sector Landing Page");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/explore-careers-content-types-migration-step-06.recipe.json", this);
                _logger.LogInformation($"Completed step 6 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 6 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }

            return 6;
        }
    }
}
