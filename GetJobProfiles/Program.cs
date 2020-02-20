using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GetJobProfiles.JsonHelpers;
using GetJobProfiles.Models.Recipe;
using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes;
using Microsoft.Extensions.Configuration;
using MoreLinq;

//todo: update existing & create new contenttypes for restrictions, other requirements etc.

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
        public const string OutputBasePath = @"i:\JobProfiles\";

        static async Task Main(string[] args)
        {
            string timestamp = $"{DateTime.UtcNow:O}";

            var socCodeConverter = new SocCodeConverter();
            var socCodeDictionary = socCodeConverter.Go(timestamp);

            //use these knobs to work around rate - limiting
            const int skip = 0;
            const int take = 30;
            const int napTimeMs = 5500;
            // max number of contentitems in an import recipe
            const int batchSize = 1000;

            var config = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .Build();

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

            var converter = new JobProfileConverter(client, socCodeDictionary, timestamp);
            await converter.Go(skip, take, napTimeMs);

            var jobProfiles = converter.JobProfiles.ToArray();

            new EscoJobProfileMapper().Map(jobProfiles);

            BatchSerializeToFiles(jobProfiles, batchSize, "JobProfiles");
            BatchSerializeToFiles(socCodeConverter.SocCodeContentItems, batchSize, "SocCodes");
            BatchSerializeToFiles(converter.Registrations.IdLookup.Select(r => new RegistrationContentItem(r.Key, timestamp, r.Key, r.Value)), batchSize, "Registrations");
            BatchSerializeToFiles(converter.Restrictions.IdLookup.Select(r => new RestrictionContentItem(r.Key, timestamp, r.Key, r.Value)), batchSize, "Restrictions");
            BatchSerializeToFiles(converter.OtherRequirements.IdLookup.Select(r => new OtherRequirementContentItem(r.Key, timestamp, r.Key, r.Value)), batchSize, "OtherRequirements");
            BatchSerializeToFiles(converter.DayToDayTasks.Select(x => new DayToDayTaskContentItem(x.Key, timestamp, x.Key, x.Value.id)), batchSize, "DayToDayTasks");
            BatchSerializeToFiles(converter.ApprenticeshipRoutes.Requirements.IdLookup.Select(r => new ApprenticeshipRequirementContentItem(r.Key, timestamp, r.Key, r.Value)), batchSize, "ApprenticeshipRequirements");
            BatchSerializeToFiles(converter.ApprenticeshipRoutes.Links.IdLookup.Select(r => new ApprenticeshipLinkContentItem(r.Key, r.Key, timestamp, r.Value)), batchSize, "ApprenticeshipLinks");
            BatchSerializeToFiles(converter.CollegeRoutes.Requirements.IdLookup.Select(r => new CollegeRequirementContentItem(r.Key, timestamp, r.Key, r.Value)), batchSize, "CollegeRequirements");
            BatchSerializeToFiles(converter.CollegeRoutes.Links.IdLookup.Select(r => new CollegeLinkContentItem(r.Key, r.Key, timestamp, r.Value)), batchSize, "CollegeLinks");
            BatchSerializeToFiles(converter.UniversityRoutes.Requirements.IdLookup.Select(r => new UniversityRequirementContentItem(r.Key, timestamp, r.Key, r.Value)), batchSize, "UniversityRequirements");
            BatchSerializeToFiles(converter.UniversityRoutes.Links.IdLookup.Select(r => new UniversityLinkContentItem(r.Key, r.Key, timestamp, r.Value)), batchSize, "UniversityLinks");

            File.WriteAllText("{OutputBasePath}manual_activity_mapping.json", JsonSerializer.Serialize(converter.DayToDayTaskExclusions));
        }

        private static void BatchSerializeToFiles<T>(IEnumerable<T> contentItems, int batchSize, string filenamePrefix)
        where T : ContentItem
        {
            var batches = contentItems.Batch(batchSize);
            int batchNumber = 0;
            foreach (var batchContentItems in batches)
            {
                //todo: async?
                var serializedContentItemBatch = SerializeContentItems(batchContentItems);
                ImportRecipe.Create($"{OutputBasePath}{filenamePrefix}{batchNumber++}.zip", WrapInNonSetupRecipe(serializedContentItemBatch));
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
    }
}
