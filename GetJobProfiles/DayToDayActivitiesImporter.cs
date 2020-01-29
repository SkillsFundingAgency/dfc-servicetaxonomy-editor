using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using CsvHelper;
using GetJobProfiles.Models.API;

namespace GetJobProfiles
{
    public class DayToDayActivitiesImporter
    {
        public void Import()
        {
            //cypher query to generate this CSV:
            //match (n:skos__Concept)--(x:esco__NodeLiteral) where n.uri =~".*isco/C\\d\\d\\d\\d$" and toLower(head(x.esco__nodeLiteral)) contains("(a)") return n.uri as Uri, head(x.esco__nodeLiteral) as Description
            using (var reader = new StreamReader(@"SeedData\day_to_day_activities.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                //this currently throws a MissingFieldException after running for a while
                //need to work out if it can be safely ignored or if there's an issue with one
                //of the records
                foreach (var item in csv.GetRecords<DayToDayActivity>())
                {
                    foreach (Match match in Regex.Matches(item.Description, @"\([a-r]\)"))
                    {
                        var startIndex = match.Index;
                        var endIndex = item.Description.IndexOfAny(new[] { ';', '.' }, startIndex);
                        var activity = item.Description.Substring(startIndex, endIndex - startIndex);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(activity);
                    }
                }
            }
        }
    }
}
