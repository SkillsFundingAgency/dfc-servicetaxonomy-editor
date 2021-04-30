using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems;

namespace GetJobProfiles.Builders
{
    public class SocCodeContentItemBuilder
    {
        public List<SocCodeContentItem> SocCodeContentItemsList { get; private set; }

        public Dictionary<string, string> SocCodesDictionary { get; set; }

        public const string UnknownSocCode = "XXXX";

        private readonly string[] _codesToProcess;
        private readonly bool _processAll;

        public SocCodeContentItemBuilder(string[] codes)
        {
            _codesToProcess = codes;
            _processAll = !_codesToProcess.Any();
        }

        public SocCodeContentItemBuilder Build(string timestamp)
        {
            using (var reader = new StreamReader(@"SeedData\soc_codes.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // read all the rows in the csv
                SocCode[] items = csv.GetRecords<SocCode>().ToArray();

                // filter out the ones we don't care about - i.e. the groups and sub groups (no Unit value)
                SocCodeContentItemsList = items
                        .Where(x => !string.IsNullOrWhiteSpace(x.Unit) && (_processAll || _codesToProcess.Contains( x.Unit )) )
                        // Convert to ContentItems
                        .Select(x => x.ToContentItem(timestamp))
                        .ToList();

                // add an 'unknown' soc code for job profiles that have a soc code not in our list of soc codes
                SocCodeContentItemsList.Add(new SocCode { Unit = UnknownSocCode, Title = "Unknown SOC Code" }.ToContentItem(timestamp));

                // return a dictionary for the JobProfileConverter to use to link the profiles to their relevant SOC codes
                SocCodesDictionary = SocCodeContentItemsList.ToDictionary(x => x.DisplayText, x => x.ContentItemId);

                return this;
            }
        }
    }
}
