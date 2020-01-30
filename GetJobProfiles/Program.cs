using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GetJobProfiles.Models.Recipe;
using Microsoft.Extensions.Configuration;

//todo: update existing & create new contenttypes for restrictions, other requirements etc.

// when we run this for real, we should run it against prod (or preprod), so that we get the current real details,
// and no test job profiles slip through the net

namespace GetJobProfiles
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            string timestamp = $"{DateTime.UtcNow:O}Z";

            var socCodeConverter = new SocCodeConverter();
            var socCodeDictionary = socCodeConverter.Go(timestamp);

            //use these knobs to work around rate - limiting
            const int skip = 0;
            const int take = 0;
            const int napTimeMs = 8000;

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

            var converter = new JobProfileConverter(client, socCodeDictionary, timestamp);
            await converter.Go(skip, take, napTimeMs);

            new EscoJobProfileMapper().Map(converter.JobProfiles);

            //todo: async
            string socCodeContentItems = JsonSerializer.Serialize(socCodeConverter.SocCodeContentItems);
            string jobProfileContentItems = JsonSerializer.Serialize(converter.JobProfiles);
            string registrationContentItems = JsonSerializer.Serialize(converter.Registrations.Select(r => new RegistrationContentItem(r.Key, timestamp, r.Key)));
            string restrictionContentItems = JsonSerializer.Serialize(converter.Restrictions.Select(r => new RestrictionContentItem(r.Key, timestamp, r.Key)));
            string otherRequirementContentItems = JsonSerializer.Serialize(converter.OtherRequirements.Select(r => new OtherRequirementContentItem(r.Key, timestamp, r.Key)));
            string dayToDayTaskContentItems = JsonSerializer.Serialize(converter.DayToDayTasks.Select(x => new DayToDayTaskContentItem(x.Key, timestamp, x.Key)));

            string contentItems = $"{socCodeContentItems}{jobProfileContentItems}{registrationContentItems}{restrictionContentItems}{otherRequirementContentItems}{dayToDayTaskContentItems}";

            Console.WriteLine(contentItems);

            File.WriteAllText(@"D:\contentitems.json", contentItems);

            File.WriteAllText(@"D:\manual_activity_mapping.json", JsonSerializer.Serialize(converter.DayToDayTaskExclusions));
        }
    }
}
