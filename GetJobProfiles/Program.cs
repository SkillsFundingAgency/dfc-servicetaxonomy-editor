using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Recipes.Executors;
using GetJobProfiles.Importers;
using GetJobProfiles.JsonHelpers;
using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Factories;
using GetJobProfiles.Models.Recipe.Fields;
using Microsoft.Extensions.Configuration;
using MoreLinq;
using MoreLinq.Extensions;
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
    static class Program
    {
        private static string OutputBasePath = @"..\..\..\..\DFC.ServiceTaxonomy.Editor\Recipes\";
        private static string MasterRecipeOutputBasePath = @"..\..\..\..\DFC.ServiceTaxonomy.Editor\MasterRecipes\";
        private const bool _zip = false;

        private static int _fileIndex = 1;
        private static readonly StringBuilder _importFilesReport = new StringBuilder();
        private static readonly StringBuilder _importTotalsReport = new StringBuilder();
        private static readonly string _executionId = Guid.NewGuid().ToString();
        private static readonly StringBuilder _recipesStep = new StringBuilder();
        private static readonly string _recipesStepExecutionId = $"Import_{_executionId}";

        private static readonly Dictionary<string, List<Tuple<string, string>>> _contentItemTitles = new Dictionary<string, List<Tuple<string, string>>>();
        private static readonly List<object> _matchingTitles = new List<object>();
        private static readonly List<object> _missingTitles = new List<object>();

        static async Task Main(string[] args)
        {
            string timestamp = $"{DateTime.UtcNow:O}";

            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .Build();

            string jobProfilesToImport = config["JobProfilesToImport"];
            bool createTestFiles = bool.Parse(config["CreateTestFiles"] ?? "False");
            string[] socCodeList = !createTestFiles ? new string[] { } : config["TestSocCodes"].Split(',');
            string[] oNetCodeList = !createTestFiles ? new string[] { } : config["TestONetCodes"].Split(',');
            string[] apprenticeshipStandardsRefList = !createTestFiles ? new string[] { } : config["TestApprenticeshipStandardReferences"].Split(',');
            string filenamePrefix = createTestFiles ? "TestData_" : "";

            var socCodeConverter = new SocCodeConverter(socCodeList);
            var socCodeDictionary = socCodeConverter.Go(timestamp);

            using var reader = new StreamReader(@"SeedData\job_profiles_updated.xlsx");
            var jobProfileWorkbook = new XSSFWorkbook(reader.BaseStream);

            var oNetConverter = new ONetConverter(oNetCodeList);
            var oNetDictionary = oNetConverter.Go(jobProfileWorkbook, timestamp);

            var titleOptionsLookup = new TitleOptionsImporter().Import(jobProfileWorkbook);

            //use these knobs to work around rate - limiting
            const int skip = 0;
            const int take = 0;
            const int napTimeMs = 5500;
            // max number of contentitems in an import recipe
            const int batchSize = 1000;
            const int jobProfileBatchSize = 200;
            const int occupationLabelsBatchSize = 5000;
            const int occupationsBatchSize = 300;
            const int skillBatchSize = 5000;

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://pp.api.nationalcareers.service.gov.uk/job-profiles/"),
                DefaultRequestHeaders =
                {
                    {"version", "v1"},
                    {"Ocp-Apim-Subscription-Key", config["Ocp-Apim-Subscription-Key"]}
                }
            };

            var client = new RestHttpClient.RestHttpClient(httpClient);
            var converter = new JobProfileConverter(client, socCodeDictionary, oNetDictionary, titleOptionsLookup, timestamp);
            await converter.Go(skip, take, napTimeMs, jobProfilesToImport);

            var jobProfiles = converter.JobProfiles.ToArray();

            List<string> mappedOccupationUris = new EscoJobProfileMapper().Map(jobProfiles);

            var jobCategoryImporter = new JobCategoryImporter();
            jobCategoryImporter.Import(jobProfileWorkbook, timestamp, jobProfiles);

            var qcfLevelBuilder = new QCFLevelBuilder();
            qcfLevelBuilder.Build(timestamp);

            var apprenticeshipStandardImporter = new ApprenticeshipStandardImporter(apprenticeshipStandardsRefList);
            apprenticeshipStandardImporter.Import(jobProfileWorkbook, timestamp, qcfLevelBuilder.QCFLevelDictionary, jobProfiles);

            const string cypherToContentRecipesPath = "CypherToContentRecipes";

            bool excludeGraphContentMutators = bool.Parse(config["ExcludeGraphContentMutators"] ?? "False");
            if (!(excludeGraphContentMutators || createTestFiles))
            {
                await CopyRecipe(cypherToContentRecipesPath, "CreateOccupationLabelNodes");
                await CopyRecipe(cypherToContentRecipesPath, "CreateOccupationPrefLabelNodes");
                await CopyRecipe(cypherToContentRecipesPath, "CreateSkillLabelNodes");
            }

            bool excludeGraphIndexMutators = bool.Parse(config["ExcludeGraphIndexMutators"] ?? "False");
            if (!(excludeGraphIndexMutators || createTestFiles))
            {
                await CopyRecipe(cypherToContentRecipesPath, "CreateFullTextSearchIndexes");
            }

            await BatchRecipes(cypherToContentRecipesPath, "CreateOccupationLabelContentItems", occupationLabelsBatchSize, "OccupationLabels", 33036);

            string whereClause = "";
            int totalOccupations = 2942;
            if (!string.IsNullOrWhiteSpace(jobProfilesToImport) && jobProfilesToImport != "*")
            {
                string uriList = string.Join(',', mappedOccupationUris.Select(u => $"'{u}'"));
                whereClause = $"where o.uri in [{uriList}]";
                totalOccupations = mappedOccupationUris.Count;
            }
            IDictionary<string, string> tokens = new Dictionary<string, string>
            {
                {"whereClause", whereClause}
            };

            await BatchRecipes(cypherToContentRecipesPath, "CreateSkillContentItems", skillBatchSize, "Skills", 13485);
            await BatchRecipes(cypherToContentRecipesPath, "CreateOccupationContentItems", occupationsBatchSize, "Occupations", totalOccupations, tokens);

            ProcessJobProfileSpreadsheet(jobProfileWorkbook);

            converter.UpdateRouteItemsWithSharedNames();

            const string contentRecipesPath = "ContentRecipes";

            if (!createTestFiles)
            {
                await CopyRecipe(contentRecipesPath, "SharedContent");
                await CopyRecipe(contentRecipesPath, "ContentHelp");
            }

            await BatchSerializeToFiles(qcfLevelBuilder.QCFLevelContentItems, batchSize, $"{filenamePrefix}QCFLevels");
            await BatchSerializeToFiles(apprenticeshipStandardImporter.ApprenticeshipStandardRouteContentItems, batchSize, $"{filenamePrefix}ApprenticeshipStandardRoutes");
            await BatchSerializeToFiles(apprenticeshipStandardImporter.ApprenticeshipStandardContentItems, batchSize, $"{filenamePrefix}ApprenticeshipStandards");
            await BatchSerializeToFiles(RouteFactory.RequirementsPrefixes.IdLookup.Select(r => new RequirementsPrefixContentItem(r.Key, timestamp, r.Key, r.Value)), batchSize, $"{filenamePrefix}RequirementsPrefixes");
            await BatchSerializeToFiles(converter.ApprenticeshipRoutes.Links.IdLookup.Select(r => new ApprenticeshipLinkContentItem(GetTitle("ApprenticeshipLink", r.Key), r.Key, timestamp, r.Value)), batchSize, $"{filenamePrefix}ApprenticeshipLinks");
            await BatchSerializeToFiles(converter.ApprenticeshipRoutes.Requirements.IdLookup.Select(r => new ApprenticeshipRequirementContentItem(GetTitle("ApprenticeshipRequirement", r.Key), timestamp, r.Key, r.Value)), batchSize, $"{filenamePrefix}ApprenticeshipRequirements");
            await BatchSerializeToFiles(converter.CollegeRoutes.Links.IdLookup.Select(r => new CollegeLinkContentItem(GetTitle("CollegeLink", r.Key), r.Key, timestamp, r.Value)), batchSize, $"{filenamePrefix}CollegeLinks");
            await BatchSerializeToFiles(converter.CollegeRoutes.Requirements.IdLookup.Select(r => new CollegeRequirementContentItem(GetTitle("CollegeRequirement", r.Key), timestamp, r.Key, r.Value)), batchSize, $"{filenamePrefix}CollegeRequirements");
            await BatchSerializeToFiles(converter.UniversityRoutes.Links.IdLookup.Select(r => new UniversityLinkContentItem(GetTitle("UniversityLink", r.Key), r.Key, timestamp, r.Value)), batchSize, $"{filenamePrefix}UniversityLinks");
            await BatchSerializeToFiles(converter.UniversityRoutes.Requirements.IdLookup.Select(r => new UniversityRequirementContentItem(GetTitle("UniversityRequirement", r.Key), timestamp, r.Key, r.Value)), batchSize, $"{filenamePrefix}UniversityRequirements");
            await BatchSerializeToFiles(converter.DayToDayTasks.Select(x => new DayToDayTaskContentItem(x.Key, timestamp, x.Key, x.Value.id)), batchSize, $"{filenamePrefix}DayToDayTasks");
            await BatchSerializeToFiles(converter.OtherRequirements.IdLookup.Select(r => new OtherRequirementContentItem(r.Key, timestamp, r.Key, r.Value)), batchSize, $"{filenamePrefix}OtherRequirements");
            await BatchSerializeToFiles(converter.Registrations.IdLookup.Select(r => new RegistrationContentItem(GetTitle("Registration", r.Key), timestamp, r.Key, r.Value)), batchSize, $"{filenamePrefix}Registrations");
            await BatchSerializeToFiles(converter.Restrictions.IdLookup.Select(r => new RestrictionContentItem(GetTitle("Restriction", r.Key), timestamp, r.Key, r.Value)), batchSize, $"{filenamePrefix}Restrictions");
            await BatchSerializeToFiles(socCodeConverter.SocCodeContentItems, batchSize, $"{filenamePrefix}SocCodes");
            await BatchSerializeToFiles(oNetConverter.ONetOccupationalCodeContentItems, batchSize, $"{filenamePrefix}ONetOccupationalCodes");
            await BatchSerializeToFiles(converter.WorkingEnvironments.IdLookup.Select(x => new WorkingEnvironmentContentItem(GetTitle("Environment", x.Key), timestamp, x.Key, x.Value)), batchSize, $"{filenamePrefix}WorkingEnvironments");
            await BatchSerializeToFiles(converter.WorkingLocations.IdLookup.Select(x => new WorkingLocationContentItem(GetTitle("Location", x.Key), timestamp, x.Key, x.Value)), batchSize, $"{filenamePrefix}WorkingLocations");
            await BatchSerializeToFiles(converter.WorkingUniforms.IdLookup.Select(x => new WorkingUniformContentItem(GetTitle("Uniform", x.Key), timestamp, x.Key, x.Value)), batchSize, $"{filenamePrefix}WorkingUniforms");

            await BatchSerializeToFiles(converter.ApprenticeshipRoute.ItemToCompositeName.Keys, batchSize, $"{filenamePrefix}ApprenticeshipRoutes");
            await BatchSerializeToFiles(converter.CollegeRoute.ItemToCompositeName.Keys, batchSize, $"{filenamePrefix}CollegeRoutes");
            await BatchSerializeToFiles(converter.UniversityRoute.ItemToCompositeName.Keys, batchSize, $"{filenamePrefix}UniversityRoutes");
            await BatchSerializeToFiles(converter.DirectRoute.ItemToCompositeName.Keys, batchSize, $"{filenamePrefix}DirectRoutes");
            await BatchSerializeToFiles(converter.OtherRoute.ItemToCompositeName.Keys, batchSize, $"{filenamePrefix}OtherRoutes");
            await BatchSerializeToFiles(converter.VolunteeringRoute.ItemToCompositeName.Keys, batchSize, $"{filenamePrefix}VolunteeringRoutes");
            await BatchSerializeToFiles(converter.WorkRoute.ItemToCompositeName.Keys, batchSize, $"{filenamePrefix}WorkRoutes");

            await BatchSerializeToFiles(jobProfiles, jobProfileBatchSize, $"{filenamePrefix}JobProfiles", CSharpContentStep.StepName);
            await BatchSerializeToFiles(jobCategoryImporter.JobCategoryContentItems, batchSize, $"{filenamePrefix}JobCategories");

            string masterRecipeName = config["MasterRecipeName"] ?? "master";

            await WriteMasterRecipesFile(masterRecipeName);
            await File.WriteAllTextAsync($"{OutputBasePath}content items count_{_executionId}.txt", @$"{_importFilesReport}# Totals
    {_importTotalsReport}");
            await File.WriteAllTextAsync($"{OutputBasePath}manual_activity_mapping_{_executionId}.json", JsonSerializer.Serialize(converter.DayToDayTaskExclusions));
            await File.WriteAllTextAsync($"{OutputBasePath}content_titles_summary_{_executionId}.json", JsonSerializer.Serialize(new { Matches = _matchingTitles.Count, Failures = _missingTitles.Count }));
            await File.WriteAllTextAsync($"{OutputBasePath}matching_content_titles_{_executionId}.json", JsonSerializer.Serialize(_matchingTitles));
            await File.WriteAllTextAsync($"{OutputBasePath}missing_content_titles_{_executionId}.json", JsonSerializer.Serialize(_missingTitles));
        }

        private static async Task CopyRecipe(string recipePath, string recipeName)
        {
            var tokens = new Dictionary<string, string>
            {
                {"recipeName", $"{recipeName}_{_executionId}"}
            };

            await CopyRecipeWithTokenisation(recipePath, recipeName, tokens);
        }

        private static void AddRecipeToRecipesStep(/*string executionId, */ string name)
        {
            _recipesStep.AppendLine($"{{ \"executionid\": \"{_recipesStepExecutionId}\", name:\"{name}_{_executionId}\" }},");
        }

        private static async Task WriteMasterRecipesFile(string masterRecipeName)
        {
            // chop off the last ','
            _recipesStep.Length -= 3;
            string content = WrapInNonSetupRecipe(_recipesStep.ToString(), _recipesStepExecutionId, "recipes", "values");
            await ImportRecipe.CreateRecipeFile($"{MasterRecipeOutputBasePath}{masterRecipeName}_{_executionId}.recipe.json", content);
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
                tokens["recipeName"] = $"{recipeName}_{_executionId}";
            }

            foreach ((string key, string value) in tokens)
            {
                recipe = recipe.Replace($"[token:{key}]", value);
            }

            string destFilename = $"{_fileIndex++:00}. {recipeName}_{_executionId}.recipe.json";

            _importFilesReport.AppendLine($"{destFilename}: {tokens.FirstOrDefault(x => x.Key == "limit").Value}");
            AddRecipeToRecipesStep(recipeName);

            await File.WriteAllTextAsync($"{OutputBasePath}{destFilename}", recipe);
        }

        private static async Task BatchSerializeToFiles<T>(
            IEnumerable<T> contentItems,
            int batchSize,
            string recipeName,
            string stepName = "ContentNoCache") where T : ContentItem
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
                    await ImportRecipe.CreateZipFile($"{OutputBasePath}{filename}", WrapInNonSetupRecipe(serializedContentItemBatch, batchRecipeNameWithExecutionId, stepName));
                }
                else
                {
                    filename = $"{_fileIndex++:00}. {batchRecipeName}_{_executionId}.recipe.json";
                    await ImportRecipe.CreateRecipeFile($"{OutputBasePath}{filename}", WrapInNonSetupRecipe(serializedContentItemBatch, batchRecipeNameWithExecutionId, stepName));
                }

                _importFilesReport.AppendLine($"{filename}: {batchContentItems.Count()}");
                AddRecipeToRecipesStep(batchRecipeName);
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

            var matchingTitle = _contentItemTitles[key].SingleOrDefault(x => x.Item2.Trim().Equals(title.Trim(), StringComparison.InvariantCultureIgnoreCase));

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
            _contentItemTitles.Add("Uniform", ProcessContentType(workbook, "Uniform", "Title", "Description"));
            _contentItemTitles.Add("Location", ProcessContentType(workbook, "Location", "Title", "Description"));
            _contentItemTitles.Add("Environment", ProcessContentType(workbook, "Environment", "Title", "Description"));
            _contentItemTitles.Add("ApprenticeshipLink", ProcessContentType(workbook, "ApprenticeshipLink", "Title", "Text"));
            _contentItemTitles.Add("ApprenticeshipRequirement", ProcessContentType(workbook, "ApprenticeshipRequirement", "Title", "Info"));
            _contentItemTitles.Add("CollegeLink", ProcessContentType(workbook, "CollegeLink", "Title", "Text"));
            _contentItemTitles.Add("CollegeRequirement", ProcessContentType(workbook, "CollegeRequirement", "Title", "Info"));
            _contentItemTitles.Add("UniversityLink", ProcessContentType(workbook, "UniversityLink", "Title", "Text"));
            _contentItemTitles.Add("UniversityRequirement", ProcessContentType(workbook, "UniversityRequirement", "Title", "Info"));
            _contentItemTitles.Add("Restriction", ProcessContentType(workbook, "Restriction", "Title", "Info"));
            _contentItemTitles.Add("Registration", ProcessContentType(workbook, "Registration", "Title", "Info"));
        }

        private static List<Tuple<string, string>> ProcessContentType(XSSFWorkbook workbook, string excelSheet,
            string columnOneName, string columnTwoName)
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
