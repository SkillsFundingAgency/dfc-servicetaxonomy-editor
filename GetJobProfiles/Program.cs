using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GetJobProfiles.Models.Recipe;
using Microsoft.Extensions.Configuration;

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
            var socCodeDictionary = await new SocCodeConverter().Go();

            //use these knobs to work around rate - limiting
            const int skip = 0;
            const int take = 10;
            const int napTimeMs = 5000;

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

            var converter = new JobProfileConverter(client, socCodeDictionary);
            await converter.Go(skip, take, napTimeMs);

            new EscoJobProfileMapper().Map(converter.JobProfiles);
            
            //todo: async
            string serializedContentItems = JsonSerializer.Serialize(converter.JobProfiles);
            Console.WriteLine(serializedContentItems);

            File.WriteAllText(@"D:\job_profiles.json", serializedContentItems);
        }
    }
}
