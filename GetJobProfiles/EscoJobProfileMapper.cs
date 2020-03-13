using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.Parts;
using MoreLinq;

namespace GetJobProfiles
{
    public class EscoJobProfileMapper
    {
        private readonly string[] _exclusions = new string[]
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
            "tv-or-film-production-runner"
        };

        public void Map(IEnumerable<JobProfileContentItem> jobProfiles)
        {
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
                        .SingleOrDefault(x => x.EponymousPart.JobProfileWebsiteUrl.Text.Split("/").Last() == item.Url);

                    if (profile != null)
                    {
                        if (_exclusions.Contains(item.Url))
                        {
                            profile.GraphLookupPart = new GraphLookupPart();
                        }
                        else
                        {
                            var title = item.EscoTitle.Split(new[] { "\r\n" }, StringSplitOptions.None).First().Trim();
                            var uri = item.EscoUri.Split(new[] { "\r\n" }, StringSplitOptions.None).First().Trim();

                            //todo: allow > 1 graphlookuppart in a contenttype: change graphlookup to named part
                            profile.GraphLookupPart = new GraphLookupPart
                            {
                                Nodes = new[]
                                {
                                    new Node { DisplayText = title, Id = uri }
                                }
                            };
                        }
                    }
                }
            }
        }
    }
}
