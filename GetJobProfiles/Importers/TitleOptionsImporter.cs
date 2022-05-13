using System.Collections.Generic;
using System.Linq;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles.Importers
{
    public class TitleOptionsImporter
    {
        public readonly Dictionary<string, string> sheetValueToFieldValue = new Dictionary<string, string>
        {
            {"", "no_title"},
            {"As Defined", "as_defined"},
            {"Prefix with a", "prefix_with_a"},
            {"Prefix with an", "prefix_with_an"},
            {"No Prefix", "no_prefix"}
        };

        public Dictionary<string, string> Import(XSSFWorkbook workbook)
        {
            var sheet = workbook.GetSheet("JobProfile");
            int dynamicTitleColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "DynamicTitlePrefix")
                .ColumnIndex;
            int urlColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "ItemDefaultUrl").ColumnIndex;

            var titleOptionsLookup = new Dictionary<string, string>();

            for (int i = 1; i <= sheet.LastRowNum; ++i)
            {
                var row = sheet.GetRow(i);
                string titleOption = sheetValueToFieldValue[row.GetCell(dynamicTitleColumnIndex).StringCellValue];
                string url = row.GetCell(urlColumnIndex).StringCellValue.TrimStart('/');

                titleOptionsLookup.Add(url, titleOption);
            }

            return titleOptionsLookup;
        }
    }
}
