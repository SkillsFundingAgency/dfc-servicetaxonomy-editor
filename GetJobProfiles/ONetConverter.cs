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

        public Dictionary<string, string> Go(string timestamp)
        {
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
                    var occupationalCode = row.GetCell(occupationalCodeColumnIndex).StringCellValue;
                    var jobProfile = row.GetCell(jobProfileColumnIndex).StringCellValue;
                    var contentItem = new ONetOccupationalCodeContentItem(occupationalCode, timestamp);

                    ONetOccupationalCodeContentItems.Add(contentItem);

                    dict.Add(jobProfile, contentItem.ContentItemId);
                }

                return dict;
            }
        }
    }
}
