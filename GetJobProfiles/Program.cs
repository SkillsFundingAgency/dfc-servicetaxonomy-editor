using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GetJobProfiles.Models;

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
                    {"Ocp-Apim-Subscription-Key", "<paste here>"}
                }
            };
            var client = new RestHttpClient.RestHttpClient(httpClient);

            var summaries = (await client.Get<JobProfileSummary[]>("summary"))
                .Where(s => s.Title != null);

            foreach (var summary in summaries)
            {
                Console.WriteLine($"Fetching {summary.Title} job profile");
                var jobProfile = await client.Get<JobProfile>(new Uri(summary.Url, UriKind.Absolute));
            }
        }
    }
}
