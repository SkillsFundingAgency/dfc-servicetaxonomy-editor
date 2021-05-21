using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Recipes.Executors;
using GetJobProfiles.Entites.Sitefinity;
using GetJobProfiles.Entities;
using GetJobProfiles.JsonHelpers;
using GetJobProfiles.Models.Containers;
using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Factories;
using GetJobProfiles.Models.Recipe.Fields;
using Microsoft.Extensions.Configuration;
using MoreLinq;
using NPOI.XSSF.UserModel;

// when we run this for real, we should run it against prod (or preprod), so that we get the current real details,
// and no test job profiles slip through the net

//todo: model as is, suggest list of improvements
// if improvement can be hidden behind api, can make change
// suggested improvements:
// extract course
// single list to select entry requirements
// split requirements into 2 parts, ie 4 gcses / including engligh, maths
// ^^ also remove the postfix for advanced apprenticeship, if can infer (or infer after other changes)
// link to actual apprenticeship framework (and then have entry requirements off that)?
// split into intermediate apprenticeship / advanced apprenticeship (could still display under 1 section with auto generation of some existing text)

//todo: only generate occupations & occupation labels required for given job profile list

//Sample appsettings.Development.json
/*
{
    "Ocp-Apim-Subscription-Key": "",
    "ExcludeGraphContentMutators": true,
    "ExcludeGraphIndexMutators": true,
    "CreateTestFiles": false,
    "TestSocCodes": "",
    "TestONetCodes": "",
    "TestApprenticeshipStandardReferences": "",
    "MasterRecipeName": "master_subset_nographmutators",
    "JobProfilesToImport": "actor, admin assistant, civil engineer, chief executive, border force officer, cabin crew, care worker, construction labourer, electrician, emergency medical dispatcher, farmer, mp, personal assistant, plumber, police officer, postman or postwoman, primary school teacher, sales assistant, social worker, waiter, train driver"
}
*/

namespace GetJobProfiles
{
    public class MasterRecipe
    {
        public string Name { get; set; }
        public StringBuilder RecipeSteps { get; set; }

        public MasterRecipe(string name)
        {
            Name = name;
            RecipeSteps = new StringBuilder();
        }
    }

    static class Program
    {
        // Constants
        private static string appSettingsFilename = "appsettings.Development.json";
        private static string RecipeOutputBasePath = @"..\..\..\..\DFC.ServiceTaxonomy.Editor\Recipes\";
        private static string MasterRecipeOutputBasePath = @"..\..\..\..\DFC.ServiceTaxonomy.Editor\MasterRecipes\";
        private const string cypherCommandRecipesPath = "CypherCommandRecipes";
        private const string contentRecipesPath = "ContentRecipes";
        private const string cypherToContentRecipesPath = "CypherToContentRecipes";
        private const bool _zip = false;
        private static int _fileIndex = 1;

        // Member initialisation
        private static readonly StringBuilder _importFilesReport = new StringBuilder();
        private static readonly StringBuilder _importTotalsReport = new StringBuilder();
        private static readonly string _executionId = Guid.NewGuid().ToString();
        private static readonly List<MasterRecipe> _masterRecipes = new List<MasterRecipe>();
        private static StringBuilder _recipesStep;
        private static readonly string _recipesStepExecutionId = $"Import_{_executionId}";
        private static readonly Dictionary<string, List<Tuple<string, string>>> _jobProfileSpreadSheetContentItemTitles = new Dictionary<string, List<Tuple<string, string>>>();
        private static readonly List<object> _matchingTitles = new List<object>();
        private static readonly List<object> _missingTitles = new List<object>();

        //static async Task Main(string[] args)
        static void Main(string[] args)
        {
            // As of 22/4/21 the JobProfile content type consists of the following content types
            // Level 1                                                                                                                                      JobCategory
            //                                                                                                                                                   |
            // Level 2                                                                                                                                       JobProfile
            //                                                                                                                                                   |
            //             +-------------+-----------------+----------------------+-------------------------+-------------+-------------+--------------+---------+-------+-----------------+-------------+-------------+-------------+-------------+-------------+-------------+-------------+
            //             |             |                 |                      |                         |             |             |              |                 |                 |             |             |             |             |             |             |             |
            // Level 3  SOCCode        ONet           University               College                Apprenticeship    Work      Volunteering     DirectRoute         Other          Registration   Restriction     Other         Digital       Working       Working       Working   Apprenticeship
            //                     Occupational          Route                  Route                     Route         Route         Route          Route             Route                                      Requirement      Skills       Environment    Location      Uniform      Standard
            //                        Code                |                      |                         |                                                   (SpecialistTraining)                                                Level                                                    |
            //                          |           +-----+-----+          +-----+-----+            +------+------+                                                                                                                                                                   +-----+-----+
            //                          |           |           |          |           |            |             |                                                                                                                                                                   |           |
            // Level 4              ONetSkill   University  University   College      College  Apprenticeship  Apprenticeship                                                                                                                                                    Apprenticeship  QCFLevel
            //                                  Requirement    Link    Requirement     Link     Requirement         Link                                                                                                                                                           Standard
            //                                                                                                                                                                                                                                                                      Route
            // Need to be mindful of the dependencies and generate the recipe files from the bottom up

            // -------------------------------------------------------------------------------------------------------------------------
            // Get the job profile dependent data
            // -------------------------------------------------------------------------------------------------------------------------

            // config info (appsettings)
            var config = GetAppSettingsConfigurationData(appSettingsFilename);

            // Get the other settings (Paths, batch sizes etc)
            var settingsModel = GetJobProfileSettingsDataModel(config);

            // Get the job profile data that isn't available on the JobProfile API from exported Excel workbook/csv files
            var referenceData = new ReferenceData().Build(settingsModel);

            var siteFinityJobProfiles = new SiteFinityJobProfile().Import(settingsModel, referenceData.JobProfileExcelWorkbook);

            return;

            // Populate the _jobProfileSpreadSheetContentItemTitles Dictionary (TODO: refactor at some point)
            ProcessJobProfileSpreadsheet(referenceData.JobProfileExcelWorkbook);

            // Legacy code (TODO: refactor at some point)
            string whereClause = "";
            string occupationMatch = "";
            int totalOccupations = 2942;
            int totalOccupationLabels = int.Parse(config["totalOccupationLabels"] ?? "33036");
            int totalSkillLabels = int.Parse(config["totalSkillLabels"] ?? "97816");
            int totalSkills = int.Parse(config["totalSkills"] ?? "13485");

            // ESCO not used as of Feb 2021, Occupation type removed but this code left just in case ESCO is reinstated.
            List<string> mappedOccupationUris = new EscoJobProfileMapper().Map(referenceData.JobProfiles.jobProfileContentItems);


            if (!string.IsNullOrWhiteSpace(settingsModel.JobProfilesToImport) && settingsModel.JobProfilesToImport != "*")
            {
                string uriList = string.Join(',', mappedOccupationUris.Select(u => $"'{u}'"));
                whereClause = $"where o.uri in [{uriList}]";
                totalOccupations = mappedOccupationUris.Count;
                occupationMatch = " (o:esco__Occupation) --> ";
            }
            IDictionary<string, string> tokens = new Dictionary<string, string>
            {
                {"whereClause", whereClause},
                {"occupationMatch", occupationMatch }
            };

            NewMasterRecipe("main");

            CreateRecipeFiles(settingsModel, referenceData);

            bool excludeGraphContentMutators = bool.Parse(config["ExcludeGraphContentMutators"] ?? "False");
            if (!(excludeGraphContentMutators || settingsModel.CreateTestFilesBool))
            {
                CopyRecipeWithTokenisation(cypherCommandRecipesPath, "CreateOccupationLabelNodes", tokens).GetAwaiter().GetResult();
                CopyRecipeWithTokenisation(cypherCommandRecipesPath, "CreateOccupationPrefLabelNodes", tokens).GetAwaiter().GetResult();
                CopyRecipeWithTokenisation(cypherCommandRecipesPath, "CreateSkillLabelNodes", tokens).GetAwaiter().GetResult();
                CopyRecipe(cypherCommandRecipesPath, "CleanUpEscoData").GetAwaiter().GetResult();
            }

            bool excludeGraphIndexMutators = bool.Parse(config["ExcludeGraphIndexMutators"] ?? "False");
            if (!(excludeGraphIndexMutators || settingsModel.CreateTestFilesBool))
            {
                CopyRecipe(cypherCommandRecipesPath, "CreateFullTextSearchIndexes").GetAwaiter().GetResult();
            }

            string masterRecipeName = settingsModel.MasterRecipeName ?? "master";

            WriteMasterRecipesFiles(masterRecipeName).GetAwaiter().GetResult();
            File.WriteAllTextAsync($"{RecipeOutputBasePath}content items count_{_executionId}.txt", @$"{_importFilesReport}# Totals {_importTotalsReport}").GetAwaiter().GetResult();
            File.WriteAllTextAsync($"{RecipeOutputBasePath}manual_activity_mapping_{_executionId}.json", JsonSerializer.Serialize(referenceData.JobProfiles.DayToDayTaskExclusions)).GetAwaiter().GetResult();
            File.WriteAllTextAsync($"{RecipeOutputBasePath}content_titles_summary_{_executionId}.json", JsonSerializer.Serialize(new { Matches = _matchingTitles.Count, Failures = _missingTitles.Count })).GetAwaiter().GetResult();
            File.WriteAllTextAsync($"{RecipeOutputBasePath}matching_content_titles_{_executionId}.json", JsonSerializer.Serialize(_matchingTitles)).GetAwaiter().GetResult();
            File.WriteAllTextAsync($"{RecipeOutputBasePath}missing_content_titles_{_executionId}.json", JsonSerializer.Serialize(_missingTitles)).GetAwaiter().GetResult();
        }

        private static void OldStuff()
        {
            //ProcessJobProfileSpreadsheet(jobProfileSpreadSheet);



            // ESCO not used as of Feb 2021, Occupation type removed but this code left just in case ESCO is reinstated.
            //const int occupationLabelsBatchSize = 1000;
            //const int occupationsBatchSize = 400;
            //const int skillBatchSize = 400;
            //const int skillLabelsBatchSize = 1000;

            //var jobProfileApiHttpClient = new HttpClient
            //{
            //    BaseAddress = new Uri("https://pp.api.nationalcareers.service.gov.uk/job-profiles/"),
            //    DefaultRequestHeaders =
            //    {
            //        {"version", "v1"},
            //        {"Ocp-Apim-Subscription-Key", config["Ocp-Apim-Subscription-Key"]}
            //    }
            //};

            //var dysacImporter = new DysacImporter(oNetConverter.ONetOccupationalCodeToSocCodeDictionary, oNetConverter.ONetOccupationalCodeContentItems);
            //using var dysacJobProfileReader = new StreamReader(@"SeedData\dysac_job_profile_mappings.xlsx");
            //var dysacJobProfileWorkbook = new XSSFWorkbook(dysacJobProfileReader.BaseStream);

            //var jobProfileApiRestHttpClient = new RestHttpClient.RestHttpClient(jobProfileApiHttpClient);
            //var jobProfileConverter = new JobProfileConverter(jobProfileApiRestHttpClient, socCodeDictionary, oNetDictionary, titleOptionsLookup, _jobProfileSpreadSheetContentItemTitles, timestamp);
            //await jobProfileConverter.Go(skip, take, napTimeMs, jobProfilesToImport);




            //var jobProfiles = jobProfileConverter.jobProfileContentItems.ToArray();

            // ESCO not used as of Feb 2021, Occupation type removed but this code left just in case ESCO is reinstated.
            //List<string> mappedOccupationUris = new EscoJobProfileMapper().Map(jobProfiles);



            //using var dysacReader = new StreamReader(@"SeedData\dysac.xlsx");
            //var dysacWorkbook = new XSSFWorkbook(dysacReader.BaseStream);

            //dysacImporter.ImportONetSkillRank(jobProfileSpreadSheet);
            //dysacImporter.ImportTraits(jobCategoryImporter.JobCategoryContentItemIdDictionary, dysacWorkbook, timestamp);
            //dysacImporter.ImportShortQuestions(dysacWorkbook, timestamp);
            //dysacImporter.ImportQuestionSet(timestamp);
            //dysacImporter.BuildONetOccupationalSkills(dysacJobProfileWorkbook);

            //const string cypherCommandRecipesPath = "CypherCommandRecipes";

            // ESCO not used as of Feb 2021 but this code left just in case ESCO is reinstated.
            //string whereClause = "";
            //string occupationMatch = "";
            //int totalOccupations = 2942;
            //int totalOccupationLabels = int.Parse(appSettings["totalOccupationLabels"] ?? "33036");
            //int totalSkillLabels = int.Parse(appSettings["totalSkillLabels"] ?? "97816");
            //int totalSkills = int.Parse(appSettings["totalSkills"] ?? "13485");

            // ESCO not used as of Feb 2021 but this code left just in case ESCO is reinstated.
            //if (!string.IsNullOrWhiteSpace(jobProfilesToImport) && jobProfilesToImport != "*")
            //{
            //    string uriList = string.Join(',', mappedOccupationUris.Select(u => $"'{u}'"));
            //    whereClause = $"where o.uri in [{uriList}]";
            //    totalOccupations = mappedOccupationUris.Count;
            //    occupationMatch = " (o:esco__Occupation) --> ";
            //}
            //IDictionary<string, string> tokens = new Dictionary<string, string>
            //{
            //    {"whereClause", whereClause},
            //    {"occupationMatch", occupationMatch }
            //};

            //NewMasterRecipe("main");

            //bool excludeGraphContentMutators = bool.Parse(appSettings["ExcludeGraphContentMutators"] ?? "False");
            // ESCO not used as of Feb 2021 but this code left just in case ESCO is reinstated.
            //if (!(excludeGraphContentMutators || createTestFiles))
            //{
            //    await CopyRecipeWithTokenisation(cypherCommandRecipesPath, "CreateOccupationLabelNodes", tokens);
            //    await CopyRecipeWithTokenisation(cypherCommandRecipesPath, "CreateOccupationPrefLabelNodes", tokens);
            //    await CopyRecipeWithTokenisation(cypherCommandRecipesPath, "CreateSkillLabelNodes", tokens);
            //    await CopyRecipe(cypherCommandRecipesPath, "CleanUpEscoData");
            //}

            //bool excludeGraphIndexMutators = bool.Parse(appSettings["ExcludeGraphIndexMutators"] ?? "False");
            //if (!(excludeGraphIndexMutators || createTestFiles))
            //{
            //    await CopyRecipe(cypherCommandRecipesPath, "CreateFullTextSearchIndexes");
            //}

            //const string cypherToContentRecipesPath = "CypherToContentRecipes";

            // ESCO not used as of Feb 2021 but this code left just in case ESCO is reinstated.
            //await BatchRecipes(cypherToContentRecipesPath, "CreateOccupationLabelContentItems", occupationLabelsBatchSize, "OccupationLabels", totalOccupationLabels, tokens);
            //await BatchRecipes(cypherToContentRecipesPath, "CreateSkillLabelContentItems", skillLabelsBatchSize, "SkillLabels", totalSkillLabels, tokens);

            //await BatchRecipes(cypherToContentRecipesPath, "CreateSkillContentItems", skillBatchSize, "Skills", totalSkills, tokens);
            //await BatchRecipes(cypherToContentRecipesPath, "CreateOccupationContentItems", occupationsBatchSize, "Occupations", totalOccupations, tokens);



            //jobProfileConverter.UpdateRouteItemsWithSharedNames();



            // remove calls to pages related recipe file generation
            // comment out as may need to re-generate later



            //await BatchSerializeToFiles(qcfLevelBuilder.QCFLevelContentItems, recipeBatchSize, $"{filenamePrefix}QCFLevels");
            //await BatchSerializeToFiles(apprenticeshipStandardImporter.ApprenticeshipStandardRouteContentItems, recipeBatchSize, $"{filenamePrefix}ApprenticeshipStandardRoutes");
            //await BatchSerializeToFiles(apprenticeshipStandardImporter.ApprenticeshipStandardContentItems, recipeBatchSize, $"{filenamePrefix}ApprenticeshipStandards");
            //await BatchSerializeToFiles(RouteFactory.RequirementsPrefixes.IdLookup.Select(r => new RequirementsPrefixContentItem(r.Key, timestamp, r.Key, r.Value)), recipeBatchSize, $"{filenamePrefix}RequirementsPrefixes");
            //await BatchSerializeToFiles(jobProfileConverter.ApprenticeshipRoutes.Links.IdLookup.Select(r => new ApprenticeshipLinkContentItem(GetTitle("ApprenticeshipLink", r.Key), r.Key, timestamp, r.Value)), recipeBatchSize, $"{filenamePrefix}ApprenticeshipLinks");
            //await BatchSerializeToFiles(jobProfileConverter.ApprenticeshipRoutes.Requirements.IdLookup.Select(r => new ApprenticeshipRequirementContentItem(GetTitle("ApprenticeshipRequirement", r.Key), timestamp, r.Key, r.Value)), recipeBatchSize, $"{filenamePrefix}ApprenticeshipRequirements");
            //await BatchSerializeToFiles(jobProfileConverter.CollegeRoutes.Links.IdLookup.Select(r => new CollegeLinkContentItem(GetTitle("CollegeLink", r.Key), r.Key, timestamp, r.Value)), recipeBatchSize, $"{filenamePrefix}CollegeLinks");
            //await BatchSerializeToFiles(jobProfileConverter.CollegeRoutes.Requirements.IdLookup.Select(r => new CollegeRequirementContentItem(GetTitle("CollegeRequirement", r.Key), timestamp, r.Key, r.Value)), recipeBatchSize, $"{filenamePrefix}CollegeRequirements");
            //await BatchSerializeToFiles(jobProfileConverter.UniversityRoutes.Links.IdLookup.Select(r => new UniversityLinkContentItem(GetTitle("UniversityLink", r.Key), r.Key, timestamp, r.Value)), recipeBatchSize, $"{filenamePrefix}UniversityLinks");
            //await BatchSerializeToFiles(jobProfileConverter.UniversityRoutes.Requirements.IdLookup.Select(r => new UniversityRequirementContentItem(GetTitle("UniversityRequirement", r.Key), timestamp, r.Key, r.Value)), recipeBatchSize, $"{filenamePrefix}UniversityRequirements");
            ////await BatchSerializeToFiles(jobProfileConverter.DayToDayTasks.Select(x => new DayToDayTaskContentItem(x.Key, timestamp, x.Key, x.Value.id)), batchSize, $"{filenamePrefix}DayToDayTasks");
            //await BatchSerializeToFiles(jobProfileConverter.OtherRequirements.IdLookup.Select(r => new OtherRequirementContentItem(r.Key, timestamp, r.Key, r.Value)), recipeBatchSize, $"{filenamePrefix}OtherRequirements");
            //await BatchSerializeToFiles(jobProfileConverter.Registrations.IdLookup.Select(r => new RegistrationContentItem(GetTitle("Registration", r.Key), timestamp, r.Key, r.Value)), recipeBatchSize, $"{filenamePrefix}Registrations");
            //await BatchSerializeToFiles(jobProfileConverter.Restrictions.IdLookup.Select(r => new RestrictionContentItem(GetTitle("Restriction", r.Key), timestamp, r.Key, r.Value)), recipeBatchSize, $"{filenamePrefix}Restrictions");
            //await BatchSerializeToFiles(socCodeConverter.SocCodeContentItems, recipeBatchSize, $"{filenamePrefix}SocCodes");

            //await CopyRecipe(contentRecipesPath, "ONetSkill");
            //await BatchSerializeToFiles(oNetConverter.ONetOccupationalCodeContentItems, recipeBatchSize, $"{filenamePrefix}ONetOccupationalCodes", CSharpContentStep.StepName);

            //await CopyRecipeWithTokenisation(cypherCommandRecipesPath, "ONetSkillMappings", new Dictionary<string, string>
            //{
            //    {"commandText", string.Join($"{Environment.NewLine},", dysacImporter.ONetSkillCypherCommands) }

            //});

            //await BatchSerializeToFiles(jobProfileConverter.WorkingEnvironments.IdLookup.Select(x => new WorkingEnvironmentContentItem(GetTitle("Environment", x.Key), timestamp, x.Key, x.Value)), recipeBatchSize, $"{filenamePrefix}WorkingEnvironments");
            //await BatchSerializeToFiles(jobProfileConverter.WorkingLocations.IdLookup.Select(x => new WorkingLocationContentItem(GetTitle("Location", x.Key), timestamp, x.Key, x.Value)), recipeBatchSize, $"{filenamePrefix}WorkingLocations");
            //await BatchSerializeToFiles(jobProfileConverter.WorkingUniforms.IdLookup.Select(x => new WorkingUniformContentItem(GetTitle("Uniform", x.Key), timestamp, x.Key, x.Value)), recipeBatchSize, $"{filenamePrefix}WorkingUniforms");

            //await BatchSerializeToFiles(jobProfileConverter.ApprenticeshipRoute.ItemToCompositeName.Keys, recipeBatchSize, $"{filenamePrefix}ApprenticeshipRoutes");
            //await BatchSerializeToFiles(jobProfileConverter.CollegeRoute.ItemToCompositeName.Keys, recipeBatchSize, $"{filenamePrefix}CollegeRoutes");
            //await BatchSerializeToFiles(jobProfileConverter.UniversityRoute.ItemToCompositeName.Keys, recipeBatchSize, $"{filenamePrefix}UniversityRoutes");
            //await BatchSerializeToFiles(jobProfileConverter.DirectRoute.ItemToCompositeName.Keys, recipeBatchSize, $"{filenamePrefix}DirectRoutes");
            //await BatchSerializeToFiles(jobProfileConverter.OtherRoute.ItemToCompositeName.Keys, recipeBatchSize, $"{filenamePrefix}OtherRoutes");
            //await BatchSerializeToFiles(jobProfileConverter.VolunteeringRoute.ItemToCompositeName.Keys, recipeBatchSize, $"{filenamePrefix}VolunteeringRoutes");
            //await BatchSerializeToFiles(jobProfileConverter.WorkRoute.ItemToCompositeName.Keys, recipeBatchSize, $"{filenamePrefix}WorkRoutes");

            //await BatchSerializeToFiles(jobProfiles, jobProfileBatchSize, $"{filenamePrefix}JobProfiles", CSharpContentStep.StepName);
            //await BatchSerializeToFiles(jobCategoryImporter.JobCategoryContentItems, recipeBatchSize, $"{filenamePrefix}JobCategories");

            // Not required for job profiles
            //await BatchSerializeToFiles(dysacImporter.PersonalityTraitContentItems, batchSize, $"{filenamePrefix}PersonalityTrait", CSharpContentStep.StepName);
            //await BatchSerializeToFiles(dysacImporter.PersonalityShortQuestionContentItems, batchSize, $"{filenamePrefix}PersonalityShortQuestion", CSharpContentStep.StepName);
            //await BatchSerializeToFiles(dysacImporter.PersonalityQuestionSetContentItems, batchSize, $"{filenamePrefix}PersonalityQuestionSet", CSharpContentStep.StepName);

            //await CopyRecipe(contentRecipesPath, "PersonalityFilteringQuestion");

            //string masterRecipeName = appSettings["MasterRecipeName"] ?? "master";

            //await WriteMasterRecipesFiles(masterRecipeName);
            //await File.WriteAllTextAsync($"{RecipeOutputBasePath}content items count_{_executionId}.txt", @$"{_importFilesReport}# Totals {_importTotalsReport}");
            //await File.WriteAllTextAsync($"{RecipeOutputBasePath}manual_activity_mapping_{_executionId}.json", JsonSerializer.Serialize(jobProfileConverter.DayToDayTaskExclusions));
            //await File.WriteAllTextAsync($"{RecipeOutputBasePath}content_titles_summary_{_executionId}.json", JsonSerializer.Serialize(new { Matches = _matchingTitles.Count, Failures = _missingTitles.Count }));
            //await File.WriteAllTextAsync($"{RecipeOutputBasePath}matching_content_titles_{_executionId}.json", JsonSerializer.Serialize(_matchingTitles));
            //await File.WriteAllTextAsync($"{RecipeOutputBasePath}missing_content_titles_{_executionId}.json", JsonSerializer.Serialize(_missingTitles));
        }

        private static void CreateRecipeFiles(
            SettingsModel settings,
            ReferenceData refData)
        {
            // Create the recipe files in the levels of the Job Profile ERD
            CreateLevel4Recipes(settings, refData);
            CreateLevel3Recipes(settings, refData);
            CreateLevel2Recipes(settings, refData);
            CreateLevel1Recipes(settings, refData);
        }

        private static SettingsModel GetJobProfileSettingsDataModel(IConfiguration appSettings)
        {
            var jobProfileSettingsDataModel = new SettingsModel();

            jobProfileSettingsDataModel.AppSettings = appSettings;

            //private static string RecipeOutputBasePath = @"..\..\..\..\DFC.ServiceTaxonomy.Editor\Recipes\";
            //private static string MasterRecipeOutputBasePath = @"..\..\..\..\DFC.ServiceTaxonomy.Editor\MasterRecipes\";
            //private static string JobProfileExcelWorkbookPath = @"SeedData\job_profiles_updated.xlsx";
            //private static string DysacJobProfileMappingExcelWorkbookPath = @"SeedData\dysac_job_profile_mappings.xlsx";
            //private static string appSettingsFilename = "appsettings.Development.json";
            //private static string jobProfileApiUri = "https://pp.api.nationalcareers.service.gov.uk/job-profiles/";
            //private const bool _zip = false;
            //private static int _fileIndex = 1;

            jobProfileSettingsDataModel.RecipeOutputBasePath = @"..\..\..\..\DFC.ServiceTaxonomy.Editor\Recipes\";
            jobProfileSettingsDataModel.MasterRecipeOutputBasePath = @"..\..\..\..\DFC.ServiceTaxonomy.Editor\MasterRecipes\";
            jobProfileSettingsDataModel.JobProfileExcelWorkbookPath = @"SeedData\job_profiles_updated.xlsx";
            jobProfileSettingsDataModel.DysacExcelWorkbookPath = @"SeedData\dysac.xlsx";
            jobProfileSettingsDataModel.DysacJobProfileMappingExcelWorkbookPath = @"SeedData\dysac_job_profile_mappings.xlsx";
            jobProfileSettingsDataModel.AppSettingsFilename = "appsettings.Development.json";
            jobProfileSettingsDataModel.JobProfileApiUri = "https://pp.api.nationalcareers.service.gov.uk/job-profiles/";
            jobProfileSettingsDataModel.Zip = false;
            jobProfileSettingsDataModel.FileIndex = 1;


            jobProfileSettingsDataModel.Timestamp = $"{DateTime.UtcNow:O}";
            jobProfileSettingsDataModel.JobProfilesToImport = appSettings["JobProfilesToImport"];
            jobProfileSettingsDataModel.MasterRecipeName = appSettings["MasterRecipeName"] ?? "master";
            jobProfileSettingsDataModel.CreateTestFilesBool = bool.Parse(appSettings["CreateTestFiles"] ?? "False");
            jobProfileSettingsDataModel.SocCodeDictionary = !jobProfileSettingsDataModel.CreateTestFilesBool ? new string[] { } : appSettings["TestSocCodes"].Split(',');
            jobProfileSettingsDataModel.OnetCodeDictionary = !jobProfileSettingsDataModel.CreateTestFilesBool ? new string[] { } : appSettings["TestONetCodes"].Split(',');
            jobProfileSettingsDataModel.ApprenticeshipStandardsRefDictionary = !jobProfileSettingsDataModel.CreateTestFilesBool ? new string[] { } : appSettings["TestApprenticeshipStandardReferences"].Split(',');
            jobProfileSettingsDataModel.FilenamePrefix = jobProfileSettingsDataModel.CreateTestFilesBool ? "TestData_" : "";
            jobProfileSettingsDataModel.RecipeBatchSize = 400;
            jobProfileSettingsDataModel.JobProfileBatchSize = 200;

            return jobProfileSettingsDataModel;
        }

        private static void CreateLevel4Recipes(SettingsModel settings, ReferenceData refData)
        {
            // Create the level 4 recipe files (from right to left on the ERD)

            // QCF Levels
            CreateQcfLevelContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.QcfLevels);

            // Apprenticeship Standard Route
            CreateApprenticeshipStandardRouteContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.ApprenticeshipStandards);

            // Apprenticeship Links
            CreateApprenticeshipLinkContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles, settings.Timestamp);

            // Apprenticeship Requirements
            CreateApprenticeshipRequirementContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles, settings.Timestamp);

            // College Links
            CreateCollegeLinkContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles, settings.Timestamp);

            // College Requirements
            CreateCollegeRequirementContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles, settings.Timestamp);

            // University Links
            CreateUniversityLinkContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles, settings.Timestamp);

            // University Requirements
            CreateUniversityRequirementContentItemsRecipes(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles, settings.Timestamp);
        }

        private static void CreateLevel3Recipes(SettingsModel settings, ReferenceData refData)
        {
            // create the level 3 recipe files (from right to left on the ERD)

            // Apprenticeship Standards
            CreateApprenticeshipStandardContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.ApprenticeshipStandards);

            // Working Uniforms
            CreateWorkingUniformContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles, settings.Timestamp);

            // Working Locations
            CreateWorkingLocationContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles, settings.Timestamp);

            // Working Environments
            CreateWorkingEnvironmentContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles, settings.Timestamp);

            // Digital Skills Levels
            CreateDigitalSkillsLevelContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.DigitalSkillsLevel, settings.Timestamp);

            // Other Requirements
            CreateOtherRequirementContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles, settings.Timestamp);

            // Restrictions
            CreateRestrictionContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles, settings.Timestamp);

            // Registrations
            CreateRegistrationContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles, settings.Timestamp);

            // Other Routes
            CreateOtherRouteContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles);

            // Direct Routes
            CreateDirecRouteContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles);

            // Volunteering Routes
            CreateVolunteeringRouteContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles);

            // Work Routes
            CreateWorkRouteContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles);

            // Apprenticeship Routes
            CreateApprencticeshipRouteContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles);

            // College Routes
            CreateCollegeRouteContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles);

            // University Routes
            CreateUniversityRouteContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles);

            // SOC Codes
            CreateSOCCodeContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.SocCodes);

            // ONet Occupational Codes (this is dependent on SOC code hence the reason it's after Soc codes)
            CreateONetOccupationalCodeContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.ONetOccCodes);
        }

        private static void CreateLevel2Recipes(SettingsModel settings, ReferenceData refData)
        {
            // create the level 2 recipe files

            // Job Profiles
            CreateJobProfileContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobProfiles);
        }

        private static void CreateLevel1Recipes(SettingsModel settings, ReferenceData refData)
        {
            // create the level 1 recipe files

            // Job Categories
            CreateJobCategoryContentItemsRecipe(settings.RecipeBatchSize, settings.FilenamePrefix, refData.JobCategories);

            // ONet Skills (this is a level 4 item but it's created by copying an existing recipe and modifying it)
            CreateONetSkillContentItemsRecipe(contentRecipesPath, settings, refData);

            // ONet Skill Mappings (this is a level 4 item but it's created by copying an existing recipe and modifying it)
            CreateONetSkillMappingContentItemsRecipe(contentRecipesPath, settings, refData);
        }

        private static void CreateJobCategoryContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobCategory jobCategory)
        {
            BatchSerializeToFiles(jobCategory.JobCategoryContentItems, recipeBatchSize, $"{filenamePrefix}JobCategories").GetAwaiter();
        }

        private static void CreateJobProfileContentItemsRecipe(int jobProfileBatchSize, string filenamePrefix, JobProfile jobProfile)
        {
            BatchSerializeToFiles(jobProfile.jobProfileContentItems.ToArray(), jobProfileBatchSize, $"{filenamePrefix}JobProfiles", CSharpContentStep.StepName).GetAwaiter();
        }

        private static void CreateApprenticeshipStandardContentItemsRecipe(int recipeBatchSize, string filenamePrefix, ApprenticeshipStandard apprenticeshipStandard)
        {
            BatchSerializeToFiles(apprenticeshipStandard.ApprenticeshipStandardContentItems, recipeBatchSize, $"{filenamePrefix}ApprenticeshipStandards").GetAwaiter();
        }

        private static void CreateWorkingUniformContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles, string timestamp)
        {
            var workingUniformContentItems = jobProfiles
                .WorkingUniforms
                .keyValuePair
                .Select(x => new WorkingUniformContentItem(GetTitle("Uniform", x.Key), timestamp, x.Key,x.Value));

            BatchSerializeToFiles(workingUniformContentItems, recipeBatchSize, $"{filenamePrefix}WorkingUniforms").GetAwaiter();
        }

        private static void CreateWorkingLocationContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles, string timestamp)
        {
            var workingLocationContentItems = jobProfiles
                .WorkingLocations
                .keyValuePair
                .Select(x => new WorkingLocationContentItem(GetTitle("Location", x.Key), timestamp, x.Key, x.Value));

            BatchSerializeToFiles(workingLocationContentItems, recipeBatchSize, $"{filenamePrefix}WorkingLocations").GetAwaiter();
        }

        private static void CreateWorkingEnvironmentContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles, string timestamp)
        {
            var workingEnvironmentContentItems = jobProfiles
                .WorkingEnvironments
                .keyValuePair
                .Select(x => new WorkingEnvironmentContentItem(GetTitle("Environment", x.Key), timestamp, x.Key, x.Value));

            BatchSerializeToFiles(workingEnvironmentContentItems, recipeBatchSize, $"{filenamePrefix}WorkingEnvironments").GetAwaiter();
        }

        private static void CreateDigitalSkillsLevelContentItemsRecipe(int recipeBatchSize, string filenamePrefix, DigitalSkillsLevel digitalSkillsLevel, string timestamp)
        {
            //string[] digitalSkillLevels =
            //{
            //    "to have a thorough understanding of computer systems and applications",
            //    "to be able to use acomputer and the main software packages confidently",
            //    "to be able to use acomputer and the main software packages competently",
            //    "to be able to carry out basic tasks on a computer or hand-held device"
            //};

            //List<DigitalSkillsLevelContentItem> digitalSkillLevelContentItems = new List<DigitalSkillsLevelContentItem>
            //{
            //    new DigitalSkillsLevelContentItem(digitalSkillLevels[0], timestamp, digitalSkillLevels[0]),
            //    new DigitalSkillsLevelContentItem(digitalSkillLevels[1], timestamp, digitalSkillLevels[1]),
            //    new DigitalSkillsLevelContentItem(digitalSkillLevels[2], timestamp, digitalSkillLevels[2]),
            //    new DigitalSkillsLevelContentItem(digitalSkillLevels[3], timestamp, digitalSkillLevels[3]),
            //};

            BatchSerializeToFiles(digitalSkillsLevel.DigitalSkillsLevelContentItems, recipeBatchSize, $"{filenamePrefix}DigitalSkillLevels").GetAwaiter();
        }

        private static void CreateOtherRequirementContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles, string timestamp)
        {
            var otherRequirementContentItems = jobProfiles
                .OtherRequirements
                .keyValuePair
                .Select(r => new OtherRequirementContentItem(r.Key, timestamp, r.Key, r.Value));

            BatchSerializeToFiles(otherRequirementContentItems, recipeBatchSize, $"{filenamePrefix}OtherRequirements").GetAwaiter();
        }

        private static void CreateRestrictionContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles, string timestamp)
        {
            var restrictionContentItems = jobProfiles.Restrictions.keyValuePair.Select(r => new RestrictionContentItem(GetTitle("Restriction", r.Key), timestamp, r.Key, r.Value));

            BatchSerializeToFiles(restrictionContentItems, recipeBatchSize, $"{filenamePrefix}Restrictions").GetAwaiter();
        }

        private static void CreateRegistrationContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles, string timestamp)
        {
            BatchSerializeToFiles(jobProfiles.Registrations.keyValuePair.Select(r => new RegistrationContentItem(GetTitle("Registration", r.Key), timestamp, r.Key, r.Value)), recipeBatchSize, $"{filenamePrefix}Registrations").GetAwaiter();
        }

        private static void CreateOtherRouteContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles)
        {
            BatchSerializeToFiles(jobProfiles.OtherRoute.ItemToCompositeName.Keys, recipeBatchSize, $"{filenamePrefix}OtherRoutes").GetAwaiter();
        }

        private static void CreateDirecRouteContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles)
        {
            BatchSerializeToFiles(jobProfiles.DirectRoute.ItemToCompositeName.Keys, recipeBatchSize, $"{filenamePrefix}DirectRoutes").GetAwaiter();
        }

        private static void CreateVolunteeringRouteContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles)
        {
            BatchSerializeToFiles(jobProfiles.VolunteeringRoute.ItemToCompositeName.Keys, recipeBatchSize, $"{filenamePrefix}VolunteeringRoutes").GetAwaiter();
        }

        private static void CreateWorkRouteContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles)
        {
            BatchSerializeToFiles(jobProfiles.WorkRoute.ItemToCompositeName.Keys, recipeBatchSize, $"{filenamePrefix}WorkRoutes").GetAwaiter();
        }

        private static void CreateApprencticeshipRouteContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles)
        {
            BatchSerializeToFiles(jobProfiles.ApprenticeshipRoute.ItemToCompositeName.Keys, recipeBatchSize, $"{filenamePrefix}ApprenticeshipRoutes").GetAwaiter();
        }

        private static void CreateCollegeRouteContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles)
        {
            BatchSerializeToFiles(jobProfiles.CollegeRoute.ItemToCompositeName.Keys, recipeBatchSize, $"{filenamePrefix}CollegeRoutes").GetAwaiter();
        }

        private static void CreateUniversityRouteContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles)
        {
            BatchSerializeToFiles(jobProfiles.UniversityRoute.ItemToCompositeName.Keys, recipeBatchSize, $"{filenamePrefix}UniversityRoutes").GetAwaiter();
        }

        private static void CreateONetOccupationalCodeContentItemsRecipe(int recipeBatchSize, string filenamePrefix, ONetOccCode oNetOccCodes)
        {
            BatchSerializeToFiles(oNetOccCodes.ONetOccupationalCodeContentItems,
                recipeBatchSize,
                $"{filenamePrefix}ONetOccupationalCodes",
                CSharpContentStep.StepName).GetAwaiter();
        }

        private static void CreateSOCCodeContentItemsRecipe(int recipeBatchSize, string filenamePrefix, SocCode socCodes)
        {
            BatchSerializeToFiles(socCodes.SocCodeContentItemsList, recipeBatchSize, $"{filenamePrefix}SocCodes").GetAwaiter();
        }

        private static void CreateQcfLevelContentItemsRecipe(int recipeBatchSize, string filenamePrefix, QcfLevel qcfLevels)
        {
            BatchSerializeToFiles(qcfLevels.QcfLevelContentItems, recipeBatchSize, $"{filenamePrefix}QCFLevels").GetAwaiter();
        }

        private static void CreateApprenticeshipStandardRouteContentItemsRecipe(int recipeBatchSize, string filenamePrefix, ApprenticeshipStandard apprenticeshipStandards)
        {
            BatchSerializeToFiles(apprenticeshipStandards.ApprenticeshipStandardRouteContentItems, recipeBatchSize, $"{filenamePrefix}ApprenticeshipStandardRoutes").GetAwaiter();
        }

        private static void CreateApprenticeshipLinkContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles, string timestamp)
        {
            var apprenticeshipLinkContentItems = jobProfiles
                .ApprenticeshipRoutes
                .Links
                .keyValuePair
                .Select(r => new ApprenticeshipLinkContentItem(GetTitle("ApprenticeshipLink", r.Key), r.Key, timestamp, r.Value));

            if (apprenticeshipLinkContentItems == null)
            {
                throw new Exception("apprenticeshipLinkContentItems is null");
            }

            BatchSerializeToFiles(apprenticeshipLinkContentItems, recipeBatchSize, $"{filenamePrefix}ApprenticeshipLinks").GetAwaiter();
        }

        private static void CreateApprenticeshipRequirementContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles, string timestamp)
        {
            var apprenticeshipRequirementContentItems = jobProfiles
                .ApprenticeshipRoutes
                .Requirements
                .keyValuePair
                .Select(r => new ApprenticeshipRequirementContentItem(GetTitle("ApprenticeshipRequirement", r.Key), timestamp, r.Key, r.Value));

            if (apprenticeshipRequirementContentItems == null)
            {
                throw new Exception("apprenticeshipRequirementContentItems is null");
            }

            BatchSerializeToFiles(apprenticeshipRequirementContentItems, recipeBatchSize, $"{filenamePrefix}ApprenticeshipRequirements").GetAwaiter();
        }

        private static void CreateCollegeLinkContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles, string timestamp)
        {
            var collegeLinkContentItems = jobProfiles
                .CollegeRoutes
                .Links
                .keyValuePair.Select(r => new CollegeLinkContentItem(GetTitle("CollegeLink", r.Key), r.Key, timestamp, r.Value));

            if (collegeLinkContentItems == null)
            {
                throw new Exception("collegeLinkContentItems is null");
            }

            BatchSerializeToFiles(collegeLinkContentItems, recipeBatchSize, $"{filenamePrefix}CollegeLinks").GetAwaiter();
        }

        private static void CreateCollegeRequirementContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles, string timestamp)
        {
            var collegeRequirementContentItems = jobProfiles
                .CollegeRoutes
                .Requirements
                .keyValuePair.Select(r => new CollegeRequirementContentItem(GetTitle("CollegeRequirement", r.Key), timestamp, r.Key, r.Value));

            if (collegeRequirementContentItems == null)
            {
                throw new Exception("collegeRequirementContentItems is null");
            }

            BatchSerializeToFiles(collegeRequirementContentItems, recipeBatchSize, $"{filenamePrefix}CollegeRequirements").GetAwaiter();
        }

        private static void CreateUniversityLinkContentItemsRecipe(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles, string timestamp)
        {
            var universitLinkContentItems = jobProfiles
                .UniversityRoutes
                .Links
                .keyValuePair.Select(r => new UniversityLinkContentItem(GetTitle("UniversityLink", r.Key), r.Key, timestamp, r.Value));

            if (universitLinkContentItems == null)
            {
                throw new Exception("universitLinkContentItems is null");
            }

            BatchSerializeToFiles(universitLinkContentItems, recipeBatchSize, $"{filenamePrefix}UniversityLinks").GetAwaiter();
        }

        private static void CreateUniversityRequirementContentItemsRecipes(int recipeBatchSize, string filenamePrefix, JobProfile jobProfiles, string timestamp)
        {
            var universityRequirementContentItems = jobProfiles
                .UniversityRoutes
                .Requirements
                .keyValuePair
                .Select(r => new UniversityRequirementContentItem(GetTitle("UniversityRequirement", r.Key), timestamp, r.Key, r.Value));

            if (universityRequirementContentItems == null)
            {
                throw new Exception("universityRequirementContentItems is null");
            }

            BatchSerializeToFiles(universityRequirementContentItems, recipeBatchSize, $"{filenamePrefix}UniversityRequirements").GetAwaiter();
        }

        private static void CreateONetSkillContentItemsRecipe(string contentRecipesPath, SettingsModel settings, ReferenceData refData)
        {
            CopyRecipe(contentRecipesPath, "ONetSkill");
        }

        private static void CreateONetSkillMappingContentItemsRecipe(string contentRecipesPath, SettingsModel settings, ReferenceData refData)
        {
            var onetSkillCypherCommandsDictionary = new Dictionary<string, string>
            {
                {
                    "commandText",
                    string.Join($"{Environment.NewLine},",
                    refData.OnetSkills.ONetSkillCypherCommands)
                }
            };

            CopyRecipeWithTokenisation(cypherCommandRecipesPath, "ONetSkillMappings", onetSkillCypherCommandsDictionary).GetAwaiter().GetResult();
        }

        private static IConfigurationRoot GetAppSettingsConfigurationData(string appSettingsFilename)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile(appSettingsFilename, optional: true)
                .Build();

            if(config == null)
            {
                throw new Exception($"Failed to get appsettings configuration data for the file {appSettingsFilename}.");
            }

            return config;
        }

        private static Task CopyRecipe(string recipePath, string recipeName)
        {
            var tokens = new Dictionary<string, string>
            {
                {"recipeName", $"{recipeName}_{_executionId}"}
            };

            return CopyRecipeWithTokenisation(recipePath, recipeName, tokens);
        }

        private static void NewMasterRecipe(string name)
        {
            var newMasterRecipe = new MasterRecipe(name);
            _masterRecipes.Add(newMasterRecipe);
            _recipesStep = newMasterRecipe.RecipeSteps;
        }

        private static void AddRecipeToRecipesStep(/*string executionId, */ string name)
        {
            _recipesStep.AppendLine($"{{ \"executionid\": \"{_recipesStepExecutionId}\", name:\"{name}_{_executionId}\" }},");
        }

        private static async Task WriteMasterRecipesFiles(string masterRecipeName)
        {
            foreach (var masterRecipe in _masterRecipes)
            {
                // chop off the last ','
                masterRecipe.RecipeSteps.Length -= 3;
                string content = WrapInNonSetupRecipe(
                    masterRecipe.RecipeSteps.ToString(), _recipesStepExecutionId, "recipes", "values");
                await ImportRecipe.CreateRecipeFile(
                    $"{MasterRecipeOutputBasePath}{masterRecipeName}_{masterRecipe.Name}_{_executionId}.recipe.json", content);
            }
        }

        private static async Task BatchRecipes(string recipePath, string recipeName, int batchSize, string nodeName, int totalItems, IDictionary<string, string> tokens = null)
        {
            int skip = 0;

            tokens ??= new Dictionary<string, string>();

            tokens["skip"] = skip.ToString();
            tokens["limit"] = batchSize.ToString();

            do
            {
                tokens["recipeName"] = $"{recipeName}_{_executionId}";

                await CopyRecipeWithTokenisation(recipePath, recipeName, tokens);

                skip += batchSize;
                tokens["skip"] = skip.ToString();

            } while (skip < totalItems);

            _importTotalsReport.AppendLine($"{nodeName}: {totalItems}");
        }

        private static async Task CopyRecipeWithTokenisation(string recipePath, string recipeName, IDictionary<string, string> tokens)
        {
            string sourceFilename = $"{recipeName}.recipe.json";
            string recipe = await File.ReadAllTextAsync(Path.Combine(recipePath, sourceFilename));

            // bit messy
            if (tokens.TryGetValue("skip", out string skip))
            {
                recipeName = $"{recipeName}{skip}";
            }
            tokens["recipeName"] = $"{recipeName}_{_executionId}";

            foreach ((string key, string value) in tokens)
            {
                recipe = recipe.Replace($"[token:{key}]", value);
            }

            string destFilename = $"{_fileIndex++:00}. {recipeName}_{_executionId}.recipe.json";

            _importFilesReport.AppendLine($"{destFilename}: {tokens.FirstOrDefault(x => x.Key == "limit").Value}");
            AddRecipeToRecipesStep(recipeName);

            await File.WriteAllTextAsync($"{RecipeOutputBasePath}{destFilename}", recipe);
        }

        private static async Task BatchSerializeToFiles<T>(
            IEnumerable<T> contentItems,
            int batchSize,
            string recipeName,
            string stepName = "ContentNoCache") where T : ContentItem
        {
            if (contentItems != null && contentItems.Any())
            {
                _importTotalsReport.AppendLine($"{recipeName}: {contentItems.Count()}");

                var batches = MoreEnumerable.Batch(contentItems, batchSize);
                int batchNumber = 0;
                foreach (var batchContentItems in batches)
                {
                    //todo: async?
                    string serializedContentItemBatch = SerializeContentItems(batchContentItems);

                    string batchRecipeName = $"{recipeName}{batchNumber++}";
                    string batchRecipeNameWithExecutionId = $"{batchRecipeName}_{_executionId}";

                    string filename;
                    if (_zip)
                    {
                        filename = $"{_fileIndex++:00}. {batchRecipeName}_{_executionId}.zip";
                        await ImportRecipe.CreateZipFile($"{RecipeOutputBasePath}{filename}", WrapInNonSetupRecipe(serializedContentItemBatch, batchRecipeNameWithExecutionId, stepName));
                    }
                    else
                    {
                        filename = $"{_fileIndex++:00}. {batchRecipeName}_{_executionId}.recipe.json";
                        await ImportRecipe.CreateRecipeFile($"{RecipeOutputBasePath}{filename}", WrapInNonSetupRecipe(serializedContentItemBatch, batchRecipeNameWithExecutionId, stepName));
                    }

                    _importFilesReport.AppendLine($"{filename}: {batchContentItems.Count()}");
                    AddRecipeToRecipesStep(batchRecipeName);
                }
            }
        }

        public static string WrapInNonSetupRecipe(string content, string name, string stepName = "ContentNoCache", string arrayName = "data")
        {
            return $@"{{
  ""name"": ""{name}"",
  ""displayName"": ""{name}"",
  ""description"": """",
  ""author"": """",
  ""website"": """",
  ""version"": """",
  ""issetuprecipe"": false,
  ""categories"": """",
  ""tags"": [],
  ""steps"": [
    {{
      ""name"": ""{stepName}"",
      ""{arrayName}"": [
{content}
      ]
    }}
  ]
}}";
        }

        private static string WrapInContent(string content)
        {
            return $@"         {{
            ""name"": ""ContentNoCache"",
            ""data"":  [
{content}
            ]
        }}
";
        }

        private static string AddComma(string contentItems)
        {
            return string.IsNullOrEmpty(contentItems) ? contentItems : $"{contentItems},";
        }

        private static string SerializeContentItems(IEnumerable<ContentItem> items)
        {
            if (!items.Any())
                return "";

            var first = items.First();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = new ContentItemJsonNamingPolicy(first.ContentType),
                Converters = { new PolymorphicWriteOnlyJsonConverter<ContentItem>() }
            };

            string itemsWithSquareBrackets = JsonSerializer.Serialize(items, options);
            return StripSquareBrackets(itemsWithSquareBrackets);
        }

        private static string StripSquareBrackets(string str)
        {
            return (str.Length > 6 && str[0] == '[') ? str.Substring(3, str.Length - 6) : str;
        }

        private static string GetTitle(string key, string title)
        {
            if (title.StartsWith("[") && title.EndsWith("]") && title.Contains("|"))
            {
                title = title.Trim('[', ']').Split('|').First().Trim();
            }
            else if (title.Contains("[") && title.Contains("|") && title.Contains("]"))
            {
                title = HtmlField.ConvertLinks(title);
            }

            var matchingTitle = _jobProfileSpreadSheetContentItemTitles[key].SingleOrDefault(x => x.Item2.Trim().Equals(title.Trim(), StringComparison.InvariantCultureIgnoreCase));

            if (matchingTitle == null)
            {
                _missingTitles.Add(new { Type = key, ExistingTitle = title });
            }
            else
            {
                _matchingTitles.Add(new { Type = key, ExistingTitle = title });
            }

            return matchingTitle?.Item1 ?? title;
        }

        private static void ProcessJobProfileSpreadsheet(XSSFWorkbook workbook)
        {
            //todo: does each of these need it's own titles?
            _jobProfileSpreadSheetContentItemTitles.Add("Uniform", ProcessContentType(workbook, "Uniform", "Title", "Description"));
            _jobProfileSpreadSheetContentItemTitles.Add("Location", ProcessContentType(workbook, "Location", "Title", "Description"));
            _jobProfileSpreadSheetContentItemTitles.Add("Environment", ProcessContentType(workbook, "Environment", "Title", "Description"));
            _jobProfileSpreadSheetContentItemTitles.Add("ApprenticeshipLink", ProcessContentType(workbook, "ApprenticeshipLink", "Title", "Text"));
            _jobProfileSpreadSheetContentItemTitles.Add("ApprenticeshipRequirement", ProcessContentType(workbook, "ApprenticeshipRequirement", "Title", "Info"));
            _jobProfileSpreadSheetContentItemTitles.Add("CollegeLink", ProcessContentType(workbook, "CollegeLink", "Title", "Text"));
            _jobProfileSpreadSheetContentItemTitles.Add("CollegeRequirement", ProcessContentType(workbook, "CollegeRequirement", "Title", "Info"));
            _jobProfileSpreadSheetContentItemTitles.Add("UniversityLink", ProcessContentType(workbook, "UniversityLink", "Title", "Text"));
            _jobProfileSpreadSheetContentItemTitles.Add("UniversityRequirement", ProcessContentType(workbook, "UniversityRequirement", "Title", "Info"));
            _jobProfileSpreadSheetContentItemTitles.Add("Restriction", ProcessContentType(workbook, "Restriction", "Title", "Info"));
            _jobProfileSpreadSheetContentItemTitles.Add("Registration", ProcessContentType(workbook, "Registration", "Title", "Info"));
            _jobProfileSpreadSheetContentItemTitles.Add("DayToDayTasks", ProcessContentType(workbook, "JobProfile", "Title", "WYDDayToDayTasks"));
            _jobProfileSpreadSheetContentItemTitles.Add("HiddenAlternativeTitles", ProcessContentType(workbook, "JobProfile", "Title", "HiddenAlternativeTitle"));
        }

        private static List<Tuple<string, string>> ProcessContentType(
            XSSFWorkbook workbook,
            string excelSheet,
            string columnOneName,
            string columnTwoName)
        {
            var sheet = workbook.GetSheet(excelSheet);
            int columnOneIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == columnOneName).ColumnIndex;
            int columnTwoIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == columnTwoName).ColumnIndex;

            var results = new List<Tuple<string, string>>();

            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                var item1 = row.GetCell(columnOneIndex).StringCellValue;
                var item2 = row.GetCell(columnTwoIndex).StringCellValue;

                results.Add(new Tuple<string, string>(item1, item2));
            }

            return results;
        }
    }
}
