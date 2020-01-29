using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using GetJobProfiles.Models.API;
using Newtonsoft.Json;
using OrchardCore.Entities;

namespace GetJobProfiles
{
    public class SocCodeConverter
    {
        private readonly DefaultIdGenerator _generator;

        public SocCodeConverter()
        {
            _generator = new DefaultIdGenerator();
        }

        public Dictionary<string, string> Go()
        {
            using (var reader = new StreamReader(@"SeedData\soc_codes.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                //read all the rows in the csv
                SocCode[] items = csv.GetRecords<SocCode>().ToArray();
                //filter out the ones we don't care about - i.e. the groups and sub groups (no Unit value)
                var contentItems = items
                    .Where(x => !string.IsNullOrWhiteSpace(x.Unit))
                    //convert to ContentItems
                    .Select(x => x.ToContentItem(_generator))
                    .ToList();

                //spit out to json file - where does this need to go?
                File.WriteAllText(@"D:\soc_codes.json", JsonConvert.SerializeObject(contentItems));

                //return a dictionary for the JobProfileConverter to use to link the profiles to their relevant SOC codes
                return contentItems.ToDictionary(x => x.DisplayText, x => x.ContentItemId);
            }
        }
    }
}
