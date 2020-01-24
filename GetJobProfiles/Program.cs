using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe;
using Microsoft.Extensions.Configuration;
using MoreLinq.Extensions;

// when we run this for real, we should run it against prod (or preprod), so that we get the current real details,
// and no test job profiles slip through the net

/* we're gonna have to zip/include graph lookup to occupation (which won't come from the job profile api
 *
 */

namespace GetJobProfiles
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            // use these knobs to work around rate-limiting
            const int skip = 0;
            const int take = 1;
            const int napTimeMs = 0;

            var config = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .Build();

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://dev.api.nationalcareersservice.org.uk/job-profiles/"),
                DefaultRequestHeaders =
                {
                    {"version", "v1"},
                    {"Ocp-Apim-Subscription-Key", config["Ocp-Apim-Subscription-Key"]}
                }
            };
            var client = new RestHttpClient.RestHttpClient(httpClient);

            var summaries = (await client.Get<JobProfileSummary[]>("summary"))
                .Where(s => s.Title != null
                            && s.Title != "Api Test Profile"
                            && s.Title != "Jas test"
                            && s.Title != "Ismail Test Profile"
                            && s.Title != "Auditor Hari Test"
                            && s.Title != "Blacksmith LJ IC Test"
                            && s.Title != "Electrical engineering technician -test"
                            && s.Title != "GP - Karl"
                            && s.Title != "mktest"
                            && s.Title != "mktest1"
                            && s.Title != "Test 2"
                            && s.Title != "Test jP Hari"
                            && s.Title != "Vehicle body repairer - Test"
                            && s.Title != "Wedding planner Testing "
                            && s.Title != "Welly designer Amended"
                            && s.Title != "Zookeeper_ilyas1"
                            && s.Title != "This is my patched breadcrumb title"
                            && s.Title != "Technical brewer - GSR3");

            if (skip > 0)
                summaries = summaries.Skip(skip);
            if (take > 0)
                summaries = summaries.Take(take);

            string timestamp = $"{DateTime.UtcNow:O}Z";

            IEnumerable<ContentItem> contentItems = Enumerable.Empty<ContentItem>();
            var summaryBatches = summaries.Batch(10);
            foreach (var batch in summaryBatches)
            {
                var contentItemBatch = await GetAndConvert(client, timestamp, batch);
                contentItems = contentItems.Concat(contentItemBatch);
                // rate-limited
                if (napTimeMs == 0)
                    continue;

                Console.WriteLine("Taking a nap");
                Thread.Sleep(napTimeMs);
            }

            //todo: async
            var serializedContentItems = JsonSerializer.Serialize(contentItems);
            Console.WriteLine(serializedContentItems);
        }

        public static async Task<IEnumerable<ContentItem>> GetAndConvert(RestHttpClient.RestHttpClient client, string timestamp, IEnumerable<JobProfileSummary> jobProfileSummaries)
        {
            var getAndConvertTasks = jobProfileSummaries.Select(s => GetAndConvert(client, timestamp, s));
            return await Task.WhenAll(getAndConvertTasks);
        }

        public static async Task<ContentItem> GetAndConvert(RestHttpClient.RestHttpClient client, string timestamp, JobProfileSummary summary)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($">>> Fetching {summary.Title} job profile");
            var jobProfile = await client.Get<JobProfile>(new Uri(summary.Url, UriKind.Absolute));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"<<< Fetched {summary.Title} job profile");

            return ConvertJobProfile(jobProfile, timestamp);
        }

        public static ContentItem ConvertJobProfile(JobProfile jobProfile, string timestamp)
        {
            var contentItem = new JobProfileContentItem
            {
                //DisplayText vs Title
                ContentItemId = "[js:uuid()]",
                ContentItemVersionId = "[js:uuid()]",
                ContentType = "JobProfile",
                DisplayText = jobProfile.Title,
                Latest = true,
                Published = true,
                ModifiedUtc = timestamp,
                PublishedUtc = timestamp,
                CreatedUtc = timestamp,
                Owner = "[js: parameters('AdminUsername')]",
                Author = "[js: parameters('AdminUsername')]",
                TitlePart = new TitlePart
                {
                    Title = jobProfile.Title
                }
            };

            return contentItem;
        }
    }
}
