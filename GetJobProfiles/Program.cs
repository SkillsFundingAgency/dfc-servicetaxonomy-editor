using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe;
using static MoreLinq.Extensions.BatchExtension;

namespace GetJobProfiles
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://dev.api.nationalcareersservice.org.uk/job-profiles/"),
                DefaultRequestHeaders =
                {
                    {"version", "v1"},
                    {"Ocp-Apim-Subscription-Key", ""}
                }
            };
            var client = new RestHttpClient.RestHttpClient(httpClient);

            var summaries = (await client.Get<JobProfileSummary[]>("summary"))
                .Where(s => s.Title != null
                            && s.Title != "Api Test Profile");

            string timestamp = $"{DateTime.UtcNow:O}Z";

            // var jobProfiles = (await Task.WhenAll(summaries
            //     .Batch(10)
            //     .Select(async b => await GetAndConvert(client, timestamp, b))))
            //     .SelectMany(j => j);

            IEnumerable<ContentItem> contentItems = Enumerable.Empty<ContentItem>();
            var summaryBatches = summaries.Batch(10);
            foreach (var batch in summaryBatches)
            {
                var contentItemBatch = await GetAndConvert(client, timestamp, batch);
                contentItems = contentItems.Concat(contentItemBatch);
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
            Console.WriteLine($">>> Fetching {summary.Title} job profile");
            var jobProfile = await client.Get<JobProfile>(new Uri(summary.Url, UriKind.Absolute));
            Console.WriteLine($"<<< Fetched {summary.Title} job profile");

            return ConvertJobProfile(jobProfile, timestamp);
        }

        public static ContentItem ConvertJobProfile(JobProfile jobProfile, string timestamp)
        {
            var contentItem = new ContentItem
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
