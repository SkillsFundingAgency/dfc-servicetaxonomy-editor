using System.Collections.Generic;
using System.Linq;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles.Entities
{
    public class HiddenAlternativeTitleImporter
    {
        public Dictionary<string, string> Import(XSSFWorkbook workbook)
        {
            var sheet = workbook.GetSheet("JobProfile");
            int hiddenAlternativeLabelColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "HiddenAlternativeTitle").ColumnIndex;
            int urlColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "ItemDefaultUrl").ColumnIndex;

            var hiddenAlternativeLabelsDictionary = new Dictionary<string, string>();

            for (int i = 1; i <= sheet.LastRowNum; ++i)
            {
                var row = sheet.GetRow(i);
                string hiddenAlternativeLabel = row.GetCell(hiddenAlternativeLabelColumnIndex).StringCellValue;
                if(string.IsNullOrEmpty(hiddenAlternativeLabel))
                {
                    continue;
                }

                string url = row.GetCell(urlColumnIndex).StringCellValue.TrimStart('/');
                hiddenAlternativeLabelsDictionary.Add(url, hiddenAlternativeLabel);
            }

            return hiddenAlternativeLabelsDictionary;
        }
    }
}
