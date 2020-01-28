using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using GetJobProfiles.Models.API;
using Newtonsoft.Json;

namespace GetJobProfiles
{
    public class SocCodeConverter
    {
        public async Task Go()
        {
            using (var reader = new StreamReader(@"SeedData\soc_codes.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                //read all the rows in the csv
                var items = csv.GetRecords<SocCode>();
                //filter out the ones we don't care about - i.e. the groups and sub groups (no Unit value)
                var contentItems = items
                    .Where(x => !string.IsNullOrWhiteSpace(x.Unit))
                    //convert to ContentItems
                    .Select(x => x.ToContentItem())
                    .ToList();

                //spit out to json file - where does this need to go?
                File.WriteAllText(@"D:\soc_codes.json", JsonConvert.SerializeObject(contentItems));
            }
        }
    }
}
