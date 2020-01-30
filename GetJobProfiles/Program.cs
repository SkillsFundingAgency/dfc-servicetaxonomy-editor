using System;
using System.Collections.Generic;
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
            string timestamp = $"{DateTime.UtcNow:O}";

            var socCodeConverter = new SocCodeConverter();
            var socCodeDictionary = socCodeConverter.Go(timestamp);

            //use these knobs to work around rate - limiting
            const int skip = 0;
            const int take = 0;
            const int napTimeMs = 10000;

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

            //todo: async
            string socCodeContentItems = SerializeContentItems(socCodeConverter.SocCodeContentItems);
            string jobProfileContentItems = SerializeContentItems(converter.JobProfiles);
            string registrationContentItems = SerializeContentItems(converter.Registrations.Select(r => new RegistrationContentItem(r.Key, timestamp, r.Key, r.Value.id)));
            string restrictionContentItems = SerializeContentItems(converter.Restrictions.Select(r => new RestrictionContentItem(r.Key, timestamp, r.Key, r.Value.id)));
            string otherRequirementContentItems = SerializeContentItems(converter.OtherRequirements.Select(r => new OtherRequirementContentItem(r.Key, timestamp, r.Key, r.Value.id)));
            string dayToDayTaskContentItems = SerializeContentItems(converter.DayToDayTasks.Select(x => new DayToDayTaskContentItem(x.Key, timestamp, x.Key, x.Value.id)));

            //todo: , if not empty
            string contentItems = $@"         {{
            ""name"": ""Content"",
            ""data"":  [
{AddComma(socCodeContentItems)}
{AddComma(jobProfileContentItems)}
{AddComma(dayToDayTaskContentItems)}
{AddComma(registrationContentItems)}
{AddComma(restrictionContentItems)}
{otherRequirementContentItems}
            ]
        }}
";

            //todo:
            // what types are required for the personalisation apis?

            Console.WriteLine(contentItems);

            File.WriteAllText(@"D:\contentitems.json", contentItems);

            File.WriteAllText(@"D:\manual_activity_mapping.json", JsonSerializer.Serialize(converter.DayToDayTaskExclusions));
        }

        private static string AddComma(string contentItems)
        {
            return string.IsNullOrEmpty(contentItems) ? contentItems : $"{contentItems},";
        }

        private static string SerializeContentItems(IEnumerable<ContentItem> items)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            if (!items.Any())
                return "";

            var first = items.First();

            options.PropertyNamingPolicy = new ContentItemJsonNamingPolicy(first.ContentType);

            string itemsWithSquareBrackets = JsonSerializer.Serialize(items, items.GetType(), options);
            return StripSquareBrackets(itemsWithSquareBrackets);
        }

        private static string StripSquareBrackets(string str)
        {
            return (str.Length > 6 && str[0] == '[') ? str.Substring(3, str.Length - 6) : str;
        }
    }
}
