using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe;

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
                foreach (var item in csv.GetRecords<EscoJobProfileMap>())
                {
                    var profile = jobProfiles.SingleOrDefault(x => x.JobProfileWebsiteUrl.Text.Contains(item.Url));

                    if (profile != null)
                    {
                        profile.GraphLookupPart = new GraphLookupPart
                        {
                            Nodes = new Node[]
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
