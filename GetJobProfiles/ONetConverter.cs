using System.Collections.Generic;
using System.IO;
using System.Linq;
using GetJobProfiles.Models.Recipe.ContentItems;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles
{
    public class ONetConverter
    {
        public List<ONetOccupationalCodeContentItem> ONetOccupationalCodeContentItems { get; private set; } = new List<ONetOccupationalCodeContentItem>();
        public static readonly string UnknownJobProfile = "Unknown";
        private static readonly string UnknownCode = "00-0000.00";
        private string CodesToProcess = "";

        public ONetConverter( string codes = "")
        {
            CodesToProcess = codes;
        }

        public Dictionary<string, string> Go(string timestamp)
        {
            string contentItemId;
            using (var reader = new StreamReader(@"SeedData\job_profiles_updated.xlsx"))
            {
                var workbook = new XSSFWorkbook(reader.BaseStream);
                var sheet = workbook.GetSheet("JobProfileSoc");
                var occupationalCodeColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "ONetOccupationalCode").ColumnIndex;
                var jobProfileColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "Description").ColumnIndex;

                var dict = new Dictionary<string, string>();

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    var occupationalCode = row.GetCell(occupationalCodeColumnIndex).StringCellValue ?? UnknownCode;

                    if ( CodesToProcess.Length == 0 || CodesToProcess.Contains(occupationalCode) ) //|| occupationalCode == UnknownCode)
                    {
                        var jobProfile = row.GetCell(jobProfileColumnIndex).StringCellValue;
                        var existingContentItem = ONetOccupationalCodeContentItems.SingleOrDefault(s => s.DisplayText == occupationalCode);

                        if (existingContentItem == null)
                        {
                            var contentItem = new ONetOccupationalCodeContentItem(occupationalCode, timestamp);
                            ONetOccupationalCodeContentItems.Add(contentItem);
                            contentItemId = contentItem.ContentItemId;
                        }
                        else
                        {
                            contentItemId = existingContentItem.ContentItemId;
                        }
                        dict.Add(jobProfile, contentItemId);
                    }
                }
                // add placeholder / unknown item
                var unknownContentItem = new ONetOccupationalCodeContentItem(UnknownCode, timestamp);
                ONetOccupationalCodeContentItems.Add(unknownContentItem);
                dict.Add(UnknownJobProfile, unknownContentItem.ContentItemId);

                return dict;
            }
        }
    }
}
