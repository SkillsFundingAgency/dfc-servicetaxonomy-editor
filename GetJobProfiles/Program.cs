using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GetJobProfiles.JsonHelpers;
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


namespace GetJobProfiles
{
    static class Program
    {
        // to delete all the ncs nodes and relationships in the graph, run..
        // match (n) where any(l in labels(n) where l starts with "ncs__") detach delete n

        private static string OutputBasePath;
        private static int FileIndex = 1;
        private static StringBuilder ImportFilesReport = new StringBuilder();
        private static StringBuilder ImportTotalsReport = new StringBuilder();

        private static Dictionary<string, List<Tuple<string, string>>> _contentItemTitles = new Dictionary<string, List<Tuple<string, string>>>();
        private static List<object> _matchingTitles = new List<object>();
        private static List<object> _missingTitles = new List<object>();

        static async Task Main(string[] args)
        {
            string timestamp = $"{DateTime.UtcNow:O}";

            var socCodeConverter = new SocCodeConverter();
            var socCodeDictionary = socCodeConverter.Go(timestamp);

            //use these knobs to work around rate - limiting
            const int skip = 0;
            const int take = 0;
            const int napTimeMs = 5500;
            // max number of contentitems in an import recipe
            const int batchSize = 1000;
            const int jobProfileBatchSize = 200;
            const int occupationsBatchSize = 300;

            var config = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .Build();

            OutputBasePath = config["OutputBasePath"];

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://pp.api.nationalcareers.service.gov.uk/job-profiles/"),
                DefaultRequestHeaders =
                {
                    {"version", "v1"},
                    {"Ocp-Apim-Subscription-Key", config["Ocp-Apim-Subscription-Key"]}
                }
            };

            string jobProfilesToImport = config["JobProfilesToImport"];

            var client = new RestHttpClient.RestHttpClient(httpClient);
            var converter = new JobProfileConverter(client, socCodeDictionary, timestamp);
            await converter.Go(skip, take, napTimeMs, jobProfilesToImport);

            var jobProfiles = converter.JobProfiles.ToArray();

            new EscoJobProfileMapper().Map(jobProfiles);

            var jobCategoryImporter = new JobCategoryImporter();
            jobCategoryImporter.Import(timestamp, jobProfiles);

            var qcfLevelBuilder = new QCFLevelBuilder();
            qcfLevelBuilder.Build(timestamp);

            var apprenticeshipStandardImporter = new ApprenticeshipStandardImporter();
            apprenticeshipStandardImporter.Import(timestamp, qcfLevelBuilder.QCFLevelDictionary, jobProfiles);

            const string cypherToContentRecipesPath = "CypherToContentRecipes";
            CopyRecipe(cypherToContentRecipesPath, "CreateOccupationLabelNodesRecipe.json");
            CopyRecipe(cypherToContentRecipesPath, "CreateOccupationPrefLabelNodesRecipe.json");
            CopyRecipe(cypherToContentRecipesPath, "CreateSkillLabelNodesRecipe.json");
            CopyRecipe(cypherToContentRecipesPath, "CreateOccupationLabelContentItemsRecipe.json");
            await BatchRecipes(cypherToContentRecipesPath, "CreateOccupationContentItemsRecipe.json", occupationsBatchSize);

            ProcessLionelsSpreadsheet();

            BatchSerializeToFiles(qcfLevelBuilder.QCFLevelContentItems, batchSize, "QCFLevels");
            BatchSerializeToFiles(apprenticeshipStandardImporter.ApprenticeshipStandardRouteContentItems, batchSize, "ApprenticeshipStandardRoutes");
            BatchSerializeToFiles(apprenticeshipStandardImporter.ApprenticeshipStandardContentItems, batchSize, "ApprenticeshipStandards");
            BatchSerializeToFiles(RouteFactory.RequirementsPrefixes.IdLookup.Select(r => new RequirementsPrefixContentItem(r.Key, timestamp, r.Key, r.Value)), batchSize, "RequirementsPrefixes");
            BatchSerializeToFiles(converter.ApprenticeshipRoutes.Links.IdLookup.Select(r => new ApprenticeshipLinkContentItem(GetTitle("ApprenticeshipLink", r.Key), r.Key, timestamp, r.Value)), batchSize, "ApprenticeshipLinks");
            BatchSerializeToFiles(converter.ApprenticeshipRoutes.Requirements.IdLookup.Select(r => new ApprenticeshipRequirementContentItem(GetTitle("ApprenticeshipRequirement", r.Key), timestamp, r.Key, r.Value)), batchSize, "ApprenticeshipRequirements");
            BatchSerializeToFiles(converter.CollegeRoutes.Links.IdLookup.Select(r => new CollegeLinkContentItem(GetTitle("CollegeLink", r.Key), r.Key, timestamp, r.Value)), batchSize, "CollegeLinks");
            BatchSerializeToFiles(converter.CollegeRoutes.Requirements.IdLookup.Select(r => new CollegeRequirementContentItem(GetTitle("CollegeRequirement", r.Key), timestamp, r.Key, r.Value)), batchSize, "CollegeRequirements");
            BatchSerializeToFiles(converter.UniversityRoutes.Links.IdLookup.Select(r => new UniversityLinkContentItem(GetTitle("UniversityLink", r.Key), r.Key, timestamp, r.Value)), batchSize, "UniversityLinks");
            BatchSerializeToFiles(converter.UniversityRoutes.Requirements.IdLookup.Select(r => new UniversityRequirementContentItem(GetTitle("UniversityRequirement", r.Key), timestamp, r.Key, r.Value)), batchSize, "UniversityRequirements");
            BatchSerializeToFiles(converter.DayToDayTasks.Select(x => new DayToDayTaskContentItem(x.Key, timestamp, x.Key, x.Value.id)), batchSize, "DayToDayTasks");
            BatchSerializeToFiles(converter.OtherRequirements.IdLookup.Select(r => new OtherRequirementContentItem(r.Key, timestamp, r.Key, r.Value)), batchSize, "OtherRequirements");
            BatchSerializeToFiles(converter.Registrations.IdLookup.Select(r => new RegistrationContentItem(GetTitle("Registration", r.Key), timestamp, r.Key, r.Value)), batchSize, "Registrations");
            BatchSerializeToFiles(converter.Restrictions.IdLookup.Select(r => new RestrictionContentItem(GetTitle("Restriction", r.Key), timestamp, r.Key, r.Value)), batchSize, "Restrictions");
            BatchSerializeToFiles(socCodeConverter.SocCodeContentItems, batchSize, "SocCodes");
            BatchSerializeToFiles(converter.WorkingEnvironments.IdLookup.Select(x => new WorkingEnvironmentContentItem(GetTitle("Environment", x.Key), timestamp, x.Key, x.Value)), batchSize, "WorkingEnvironments");
            BatchSerializeToFiles(converter.WorkingLocations.IdLookup.Select(x => new WorkingLocationContentItem(GetTitle("Location", x.Key), timestamp, x.Key, x.Value)), batchSize, "WorkingLocations");
            BatchSerializeToFiles(converter.WorkingUniforms.IdLookup.Select(x => new WorkingUniformContentItem(GetTitle("Uniform", x.Key), timestamp, x.Key, x.Value)), batchSize, "WorkingUniforms");
            BatchSerializeToFiles(jobProfiles, jobProfileBatchSize, "JobProfiles");
            BatchSerializeToFiles(jobCategoryImporter.JobCategoryContentItems, batchSize, "JobCategories");

            File.WriteAllText($"{OutputBasePath}content items count.txt", @$"{ImportFilesReport}
# Totals
{ImportTotalsReport}");
            File.WriteAllText($"{OutputBasePath}manual_activity_mapping.json", JsonSerializer.Serialize(converter.DayToDayTaskExclusions));
            File.WriteAllText($"{OutputBasePath}content_titles_summary.json", JsonSerializer.Serialize(new { Matches = _matchingTitles.Count, Failures = _missingTitles.Count }));
            File.WriteAllText($"{OutputBasePath}matching_content_titles.json", JsonSerializer.Serialize(_matchingTitles));
            File.WriteAllText($"{OutputBasePath}missing_content_titles.json", JsonSerializer.Serialize(_missingTitles));
        }

        private static void CopyRecipe(string recipePath, string recipeFilename)
        {
            File.Copy(Path.Combine(recipePath, recipeFilename), $"{OutputBasePath}{FileIndex++:00}. {recipeFilename}");
        }

        private static async Task BatchRecipes(string recipePath, string recipeFilename, int batchSize)
        {
            const int totalOccupations = 2942;
            int skip = 0;

            var tokens = new Dictionary<string, string>
            {
                {"skip", skip.ToString()},
                {"limit", batchSize.ToString()}
            };

            do
            {
                await CopyRecipeWithTokenisation(recipePath, recipeFilename, tokens);

                skip += batchSize;
                tokens["skip"] = skip.ToString();

            } while (skip < totalOccupations);
        }

        private static async Task CopyRecipeWithTokenisation(string recipePath, string recipeFilename, IDictionary<string, string> tokens)
        {
            string recipe = await File.ReadAllTextAsync(Path.Combine(recipePath, recipeFilename));
            foreach ((string key, string value) in tokens)
            {
                recipe = recipe.Replace($"[token:{key}]", value);
            }

            await File.WriteAllTextAsync($"{OutputBasePath}{FileIndex++:00}. {recipeFilename}", recipe);
        }

        private static void BatchSerializeToFiles<T>(IEnumerable<T> contentItems, int batchSize, string filenamePrefix) where T : ContentItem
        {
            ImportFilesReport.AppendLine($"{filenamePrefix}: {contentItems.Count()}");

            var batches = contentItems.Batch(batchSize);
            int batchNumber = 0;
            foreach (var batchContentItems in batches)
            {
                //todo: async?
                string serializedContentItemBatch = SerializeContentItems(batchContentItems);

                string filename = $"{FileIndex++:00}. {filenamePrefix}{batchNumber++}.zip";
                ImportFilesReport.AppendLine($"{filename}: {batchContentItems.Count()}");

                ImportRecipe.Create($"{OutputBasePath}{filename}", WrapInNonSetupRecipe(serializedContentItemBatch));
            }
        }

        public static string WrapInNonSetupRecipe(string content)
        {
            return $@"{{
  ""name"": """",
  ""displayName"": """",
  ""description"": """",
  ""author"": """",
  ""website"": """",
  ""version"": """",
  ""issetuprecipe"": false,
  ""categories"": """",
  ""tags"": [],
  ""steps"": [
    {{
      ""name"": ""Content"",
      ""data"": [
{content}
      ]
    }}
  ]
}}";
        }

        private static string WrapInContent(string content)
        {
            return $@"         {{
            ""name"": ""Content"",
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
                _missingTitles.Add(new {Type = key, ExistingTitle = title});
            }
            else
            {
                _matchingTitles.Add(new {Type = key, ExistingTitle = title});
            }

            return matchingTitle?.Item1 ?? title;
        }

        private static void ProcessLionelsSpreadsheet()
        {
            _contentItemTitles.Add("Uniform", ProcessContentType("Uniform", "Title", "Description"));
            _contentItemTitles.Add("Location", ProcessContentType("Location", "Title", "Description"));
            _contentItemTitles.Add("Environment", ProcessContentType("Environment", "Title", "Description"));
            _contentItemTitles.Add("ApprenticeshipLink", ProcessContentType("ApprenticeshipLink", "Title", "Text"));
            _contentItemTitles.Add("ApprenticeshipRequirement", ProcessContentType("ApprenticeshipRequirement", "Title", "Info"));
            _contentItemTitles.Add("CollegeLink", ProcessContentType("CollegeLink", "Title", "Text"));
            _contentItemTitles.Add("CollegeRequirement", ProcessContentType("CollegeRequirement", "Title", "Info"));
            _contentItemTitles.Add("UniversityLink", ProcessContentType("UniversityLink", "Title", "Text"));
            _contentItemTitles.Add("UniversityRequirement", ProcessContentType("UniversityRequirement", "Title", "Info"));
            _contentItemTitles.Add("Restriction", ProcessContentType("Restriction", "Title", "Info"));
            _contentItemTitles.Add("Registration", ProcessContentType("Registration", "Title", "Info"));
        }

        private static List<Tuple<string, string>> ProcessContentType(string excelSheet, string columnOneName, string columnTwoName)
        {
            using (var reader = new StreamReader(@"SeedData\job_profiles_updated.xlsx"))
            {
                var workbook = new XSSFWorkbook(reader.BaseStream);
                var sheet = workbook.GetSheet(excelSheet);
                var columnOneIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == columnOneName).ColumnIndex;
                var columnTwoIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == columnTwoName).ColumnIndex;

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
}
