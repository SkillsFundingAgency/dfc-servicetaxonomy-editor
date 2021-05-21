//using System.Collections.Generic;
//using System.Linq;
//using NPOI.XSSF.UserModel;

//namespace GetJobProfiles.Builders
//{
//    public class WhatYouWillDoDayToDayTasksContentItemBuilder
//    {
//        public Dictionary<string, string> WhatYouWillDoDayToDayTasksContentItemsDictionary
//        {
//            get;
//            private set;
//        } = new Dictionary<string, string>();

//        public WhatYouWillDoDayToDayTasksContentItemBuilder Build(XSSFWorkbook workbook)
//        {
//            var sheet = workbook.GetSheet("JobProfile");
//            int dayToDayTasksColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "WYDDayToDayTasks").ColumnIndex;
//            int urlColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "ItemDefaultUrl").ColumnIndex;

//            for (int i = 1; i <= sheet.LastRowNum; ++i)
//            {
//                var row = sheet.GetRow(i);
//                string digitalSkillsLevel = row.GetCell(dayToDayTasksColumnIndex).StringCellValue;
//                if(string.IsNullOrEmpty(digitalSkillsLevel))
//                {
//                    continue;
//                }

//                string url = row.GetCell(urlColumnIndex).StringCellValue.TrimStart('/');
//                WhatYouWillDoDayToDayTasksContentItemsDictionary.Add(url, digitalSkillsLevel);
//            }

//            return this;
//        }
//    }
//}
