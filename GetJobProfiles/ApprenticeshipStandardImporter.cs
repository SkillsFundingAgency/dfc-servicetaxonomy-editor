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
using System.Text.RegularExpressions;

namespace GetJobProfiles
{
    public class ApprenticeshipStandardImporter
    {
        private static readonly DefaultIdGenerator _generator = new DefaultIdGenerator();
        private Dictionary<string, string> _standardsDictionary = new Dictionary<string, string>();
        private Dictionary<string, string> _socCodeDictionary;

        public IEnumerable<ApprenticeshipStandardContentItem> ApprenticeshipStandardContentItems { get; private set; }
        public IEnumerable<ApprenticeshipStandardRouteContentItem> ApprenticeshipStandardRouteContentItems { get; private set; }

        public void Import(string timestamp, Dictionary<string, string> qcfLevelDictionary, IEnumerable<JobProfileContentItem> jobProfiles, Dictionary<string,string> socCodeDictionary)
        {
            this._socCodeDictionary = socCodeDictionary;

            var apprenticeshipStandards = ReadStandardsFromFile();

            //IDs change every time a collection is expanded, so set IDs from a dictionary
            var routeDictionary = apprenticeshipStandards.SelectMany(x => x.Route).Distinct().Select(x => new { Id = _generator.GenerateUniqueId(), Title = x }).ToDictionary(y => y.Title, y => y.Id);
            _standardsDictionary = apprenticeshipStandards.Select(standard => new { Id = _generator.GenerateUniqueId(), Title = standard.Name }).ToDictionary(y => y.Title, y => y.Id);

            ApprenticeshipStandardRouteContentItems = apprenticeshipStandards.SelectMany(x => x.Route).Distinct().Select(route => new ApprenticeshipStandardRouteContentItem(route, timestamp, routeDictionary[route]));

            ApprenticeshipStandardContentItems = apprenticeshipStandards.Select(standard => new ApprenticeshipStandardContentItem(standard.Name, timestamp, _standardsDictionary[standard.Name])
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

            AssignStandardsToJobProfiles(jobProfiles);
        }

        public IEnumerable<ApprenticeshipStandard> ReadStandardsFromFile()
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
                var statusIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "Status").ColumnIndex;

                var apprenticeshipStandardList = new List<ApprenticeshipStandard>();

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);

                    var status = row.GetCell(statusIndex).StringCellValue;

                    //What to do if a Job Profile has a standard that's not approved? -- Will not associate at the moment
                    if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                    {
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

                        return apprenticeshipStandardList;
                    }
                }
            }

            return new List<ApprenticeshipStandard>();
        }

        private void AssignStandardsToJobProfiles(IEnumerable<JobProfileContentItem> jobProfiles)
        {
            //Read soc code tab
            using (var reader = new StreamReader(@"SeedData\job_profiles.xlsx"))
            {
                var workbook = new XSSFWorkbook(reader.BaseStream);
                var sheet = workbook.GetSheet("JobProfileSoc");
                var apprenticeshipStandardsIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "apprenticeshipstandards").ColumnIndex;
                var socCodeIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "SOCCode").ColumnIndex;

                var socStandards = new Dictionary<string, string[]>();

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);

                    var socCode = row.GetCell(socCodeIndex).StringCellValue;

                    //Remove level text
                    var standards = row.GetCell(apprenticeshipStandardsIndex).StringCellValue;
                    var cleansedStandards = Regex.Replace(standards,@"\s+\(.*?\)", "");
                    var splitStandards = cleansedStandards.Split(',');

                    socStandards.Add(socCode, splitStandards);
                }

                foreach(var jobProfile in jobProfiles)
                {
                    var jobProfileSocCode = _socCodeDictionary.FirstOrDefault(x=>x.Value == jobProfile.EponymousPart.SOCCode.ContentItemIds.FirstOrDefault()).Key;

                    //Can't retrieve by key as not all SOC codes are in the standards dictionary
                    var applicableStandards = socStandards.ContainsKey(jobProfileSocCode) ? socStandards[jobProfileSocCode] : null;

                    if (applicableStandards != null)
                    {

                    }
                }
            }

            //Lookup content ID

            //Assign
        }

      
    }
}

