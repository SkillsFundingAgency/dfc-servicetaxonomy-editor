using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe;
using GetJobProfiles.Models.Recipe.Parts;
using MoreLinq;

namespace GetJobProfiles
{
    public class EscoJobProfileMapper
    {
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
                        //todo: allow >1 graphlookuppart in a contenttype
                        profile.GraphLookupPart = new GraphLookupPart
                        {
                            Nodes = new[]
                            {
                                new Node { DisplayText = item.EscoTitle, Id = item.EscoUri }
                            }
                        };
                    }
                }
            }
        }
    }
}
