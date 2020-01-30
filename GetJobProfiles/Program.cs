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

            new EscoJobProfileMapper().Map(converter.JobProfiles);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            //todo: async
            string socCodeContentItems = JsonSerializer.Serialize(socCodeConverter.SocCodeContentItems, options);
            string jobProfileContentItems = JsonSerializer.Serialize(converter.JobProfiles, options);
            string registrationContentItems = JsonSerializer.Serialize(converter.Registrations.Select(r => new RegistrationContentItem(r.Key, timestamp, r.Key, r.Value.id)), options);
            string restrictionContentItems = JsonSerializer.Serialize(converter.Restrictions.Select(r => new RestrictionContentItem(r.Key, timestamp, r.Key, r.Value.id)), options);
            string otherRequirementContentItems = JsonSerializer.Serialize(converter.OtherRequirements.Select(r => new OtherRequirementContentItem(r.Key, timestamp, r.Key, r.Value.id)), options);
            string dayToDayTaskContentItems = JsonSerializer.Serialize(converter.DayToDayTasks.Select(x => new DayToDayTaskContentItem(x.Key, timestamp, x.Key, x.Value.id)), options);

            string contentItems = $@"         {{
            ""name"": ""Content"",
            ""data"":  [
{StripSquareBrackets(jobProfileContentItems)},
{StripSquareBrackets(socCodeContentItems)},
{StripSquareBrackets(registrationContentItems)},
{StripSquareBrackets(restrictionContentItems)},
{StripSquareBrackets(otherRequirementContentItems)},
{StripSquareBrackets(dayToDayTaskContentItems)}
            ]
        }}
";

            Console.WriteLine(contentItems);

            File.WriteAllText(@"D:\contentitems.json", contentItems);

            File.WriteAllText(@"D:\manual_activity_mapping.json", JsonSerializer.Serialize(converter.DayToDayTaskExclusions));
        }

        private static string StripSquareBrackets(string str)
        {
            return (str.Length > 6 && str[0] == '[') ? str.Substring(3, str.Length - 6) : str;
        }
    }
}
