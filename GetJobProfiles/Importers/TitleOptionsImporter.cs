using System.Collections.Generic;
using System.Linq;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles.Importers
{
    public class TitleOptionsImporter
    {
        public Dictionary<string, string> Import(XSSFWorkbook workbook)
        {
            var sheet = workbook.GetSheet("JobProfile");
            int dynamicTitleColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "DynamicTitlePrefix")
                .ColumnIndex;
            int titleColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "Title").ColumnIndex;

            var titleOptionsLookup = new Dictionary<string, string>();

            for (int i = 1; i <= sheet.LastRowNum; ++i)
            {
                var row = sheet.GetRow(i);
                string categories = row.GetCell(dynamicTitleColumnIndex).StringCellValue;
                string uri = row.GetCell(titleColumnIndex).StringCellValue.TrimStart('/');

                titleOptionsLookup.Add(uri, categories);
            }

            return titleOptionsLookup;
        }
    }
}
