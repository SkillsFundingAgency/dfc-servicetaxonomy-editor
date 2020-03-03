using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GetJobProfiles.Models.Recipe.ContentItems;
using NPOI.XSSF.UserModel;
using System.Linq;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.Parts;
using GetJobProfiles.Models.Recipe.Fields;
using OrchardCore.Entities;

namespace GetJobProfiles
{
    public class ApprenticeshipStandardImporter
    {
        private static readonly DefaultIdGenerator _generator = new DefaultIdGenerator();
        public IEnumerable<ApprenticeshipStandardContentItem> ApprenticeshipStandardContentItems { get; private set; }
        public IEnumerable<ApprenticeshipStandardRouteContentItem> ApprenticeshipStandardRouteContentItems { get; private set; }

        public void Import(string timestamp, Dictionary<string,string> qcfLevelDictionary)
        {
            using (var reader = new StreamReader(@"SeedData\ApprenticeshipStandards.xlsx"))
            {
                var workbook = new XSSFWorkbook(reader.BaseStream);
                var sheet = workbook.GetSheet("ApprenticeshipStandards");

                var nameIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "Name").ColumnIndex;
                var referenceIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "Reference").ColumnIndex;
                var levelIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "Level").ColumnIndex;
                var maximumFundingIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "Maximum Funding (£)").ColumnIndex;
                //Duration is in months
                var typicalDurationIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "Typical Duration").ColumnIndex;
                var larsCodeIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "LARS code for providers only").ColumnIndex;
                var routeIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "Route").ColumnIndex;

                var apprenticeshipStandardList = new List<ApprenticeshipStandard>();

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    var name = row.GetCell(nameIndex).StringCellValue;
                    var reference = row.GetCell(referenceIndex).StringCellValue;
                    var level = row.GetCell(levelIndex).CellType == NPOI.SS.UserModel.CellType.Numeric ? row.GetCell(levelIndex).NumericCellValue : 0;
                    var maximumFunding = row.GetCell(maximumFundingIndex).CellType == NPOI.SS.UserModel.CellType.Numeric ? row.GetCell(maximumFundingIndex).NumericCellValue : 0;
                    var typicalDuration = row.GetCell(typicalDurationIndex).NumericCellValue;
                    var larsCode = row.GetCell(larsCodeIndex).CellType == NPOI.SS.UserModel.CellType.Numeric ? row.GetCell(larsCodeIndex).NumericCellValue : 0;
                    var route = row.GetCell(routeIndex).StringCellValue;

                    apprenticeshipStandardList.Add(new ApprenticeshipStandard
                    {
                        Name = name,
                        Reference = reference,
                        Level = Convert.ToInt32(level),
                        MaximumFunding = Convert.ToInt32(maximumFunding),
                        Duration = Convert.ToInt32(typicalDuration),
                        LARSCode = Convert.ToInt32(larsCode),
                        Route = route.Split(',').Select(z => $"{char.ToUpper(z.Trim()[0]) + z.Trim().Substring(1)}").ToArray()
                    });
                }

                //IDs change every time a collection is expanded, so set IDs from a dictionary
                var routeDictionary = apprenticeshipStandardList.SelectMany(x => x.Route).Distinct().Select(x=> new { Id = _generator.GenerateUniqueId(), Title = x }).ToDictionary(y=>y.Title, y=>y.Id);

                ApprenticeshipStandardRouteContentItems = apprenticeshipStandardList.SelectMany(x => x.Route).Distinct().Select(route => new ApprenticeshipStandardRouteContentItem(route, timestamp, routeDictionary[route]));

                ApprenticeshipStandardContentItems = apprenticeshipStandardList.Select(standard => new ApprenticeshipStandardContentItem(standard.Name, timestamp)
                {
                    EponymousPart = new ApprenticeshipStandardPart
                    {
                        Reference = new TextField(standard.Reference),
                        MaximumFunding = new NumericField(standard.MaximumFunding),
                        LARSCode = new NumericField(standard.LARSCode),
                        Duration = new NumericField(standard.Duration),
                        ApprenticeshipStandardRoutes = new ContentPicker
                        {
                            ContentItemIds = routeDictionary.Where(x => standard.Route.Any(z => z == x.Key)).Select(y => y.Value)
                        },
                        QCFLevel = new ContentPicker
                        {
                            ContentItemIds = new List<string> { qcfLevelDictionary[standard.Level.ToString()] }
                        }
                    }
                });
            }
        }
    }
}
