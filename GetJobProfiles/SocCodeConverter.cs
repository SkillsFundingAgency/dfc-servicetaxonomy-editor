using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems;

namespace GetJobProfiles
{
    public class SocCodeConverter
    {
        public List<SocCodeContentItem> SocCodeContentItems { get; private set; }

        public Dictionary<string, string> Go(string timestamp)
        {
            using (var reader = new StreamReader(@"SeedData\soc_codes.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                //read all the rows in the csv
                SocCode[] items = csv.GetRecords<SocCode>().ToArray();
                //filter out the ones we don't care about - i.e. the groups and sub groups (no Unit value)
                SocCodeContentItems = items
                    .Where(x => !string.IsNullOrWhiteSpace(x.Unit))
                    //convert to ContentItems
                    .Select(x => x.ToContentItem(timestamp))
                    .ToList();

                //return a dictionary for the JobProfileConverter to use to link the profiles to their relevant SOC codes
                return SocCodeContentItems.ToDictionary(x => x.DisplayText, x => x.ContentItemId);
            }
        }
    }
}
