using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.Fields;
using MoreLinq;

namespace GetJobProfiles.Importers
{
    public class EscoJobProfileMapper
    {
        public readonly string[] _exclusions = new string[]
        {
            "arts-administrator",
            "colour-therapist",
            "dental-therapist",
            "design-and-development-engineer",
            "digital-delivery-manager",
            "drone-pilot",
            "education-technician",
            "fence-installer",
            "financial-services-customer-adviser",
            "lock-keeper",
            "photographic-stylist",
            "road-traffic-accident-investigator",
            "royal-navy-rating",
            "sonographer",
            "speech-and-language-therapy-assistant",
            "tv-or-film-production-runner",
            "ismai;-profile",
            "ismail-profile",
            "ceiling-fixer",
            "fenestration-fabricator",
            "window-fabricator",
            "heritage-officer",
            "desk-profiler"
        };

        public List<string> Map(IEnumerable<JobProfileContentItem> jobProfiles)
        {
            List<string> mappedOccupationUris = new List<string>();

            using (var reader = new StreamReader(@"SeedData\esco_job_profile_map.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                //read all the rows in the csv
                var escoJobProfileMap = csv.GetRecords<EscoJobProfileMap>().ToArray().OrderBy(jp => jp.Url);
                var distinctEscoJobProfileMap = escoJobProfileMap.DistinctBy(jp => jp.Url).OrderBy(jp => jp.Url);

                if (escoJobProfileMap.Count() != distinctEscoJobProfileMap.Count())
                {
                    ColorConsole.WriteLine($"{escoJobProfileMap.Count() - distinctEscoJobProfileMap.Count()} duplicate job profiles in map", ConsoleColor.Black, ConsoleColor.Red);
                }

                foreach (var item in distinctEscoJobProfileMap)
                {
                    JobProfileContentItem profile = jobProfiles
                        .SingleOrDefault(x => x.PageLocationPart.FullUrl?.Split("/").Last() == item.Url);

                    if (profile != null && !_exclusions.Contains(item.Url))
                    {
                        string title = item.EscoTitle.Split(new[] { "\r\n" }, StringSplitOptions.None).First().Trim();
                        string uri = item.EscoUri.Split(new[] { "\r\n" }, StringSplitOptions.None).First().Trim();

                        //todo: GetContentItemIdByUri would be better - even better GetContentItemIdByUserId and take into account settings
                        profile.EponymousPart.Occupation = new ContentPicker {ContentItemIds = new[]
                        {
                            $"«c#: await Content.GetContentItemIdByDisplayText(\"Occupation\", \"{title}\")»"
                        }};

                        mappedOccupationUris.Add(uri);
                    }
                }
            }

            return mappedOccupationUris;
        }
    }
}
