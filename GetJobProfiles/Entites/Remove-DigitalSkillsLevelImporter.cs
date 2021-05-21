using System.Collections.Generic;
using System.Linq;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles.Entities
{
    public class DigitalSkillsLevelImporter
    {
        public Dictionary<string, string> Import(XSSFWorkbook workbook)
        {
            var sheet = workbook.GetSheet("JobProfile");
            int digitalSkillsLevelColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "DigitalSkillsLevel").ColumnIndex;
            int urlColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "ItemDefaultUrl").ColumnIndex;

            var digitalSkillsLevelDictionary = new Dictionary<string, string>();

            for (int i = 1; i <= sheet.LastRowNum; ++i)
            {
                var row = sheet.GetRow(i);
                string digitalSkillsLevel = row.GetCell(digitalSkillsLevelColumnIndex).StringCellValue;
                if(string.IsNullOrEmpty(digitalSkillsLevel))
                {
                    continue;
                }

                string url = row.GetCell(urlColumnIndex).StringCellValue.TrimStart('/');
                digitalSkillsLevelDictionary.Add(url, digitalSkillsLevel);
            }

            return digitalSkillsLevelDictionary;
        }
    }
}
