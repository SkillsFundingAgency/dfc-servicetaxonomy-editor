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
using NPOI.SS.UserModel;
using Newtonsoft.Json;

namespace GetJobProfiles
{
    public class ApprenticeshipStandardImporter
    {
        private static readonly DefaultIdGenerator _generator = new DefaultIdGenerator();
        private Dictionary<string, string> _standardsDictionary = new Dictionary<string, string>();
        private Dictionary<string, string> _replacementsDictionary = new Dictionary<string, string>();

        public IEnumerable<ApprenticeshipStandardContentItem> ApprenticeshipStandardContentItems { get; private set; }
        public IEnumerable<ApprenticeshipStandardRouteContentItem> ApprenticeshipStandardRouteContentItems { get; private set; }

        /// <summary>
        /// The import routine for Apprenticeship Standards
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="qcfLevelDictionary"></param>
        /// <param name="jobProfiles"></param>
        public void Import(string timestamp, Dictionary<string, string> qcfLevelDictionary, IEnumerable<JobProfileContentItem> jobProfiles)
        {
            BuildReplacementsDictionary();
            var apprenticeshipStandards = ReadStandardsFromFile();

            var routeDictionary = apprenticeshipStandards.Where(x => x.Route != null).SelectMany(x => x.Route).Distinct().Select(x => new { Id = _generator.GenerateUniqueId(), Title = x }).ToDictionary(y => y.Title, y => y.Id);
            _standardsDictionary = apprenticeshipStandards.Select(standard => new { Id = _generator.GenerateUniqueId(), Title = standard.Name }).ToDictionary(y => y.Title, y => y.Id);

            ApprenticeshipStandardRouteContentItems = apprenticeshipStandards.Where(x => x.Route != null).SelectMany(x => x.Route).Distinct().Select(route => new ApprenticeshipStandardRouteContentItem(route, timestamp, routeDictionary[route]));
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
                        ContentItemIds = routeDictionary.Where(x => standard.Route != null && standard.Route.Any(z => z == x.Key)).Select(y => y.Value)
                    },
                    QCFLevel = new ContentPicker
                    {
                        ContentItemIds = standard.Level.HasValue ? new List<string> { qcfLevelDictionary[standard.Level.ToString()] } : new List<string>()
                    },
                    Type = new TextField(standard.Type)
                }
            });

            AssignStandardsToJobProfiles(jobProfiles);
        }

        /// <summary>
        /// Read Apprenticeship Standards and populate ApprenticeshipStandard objects
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ApprenticeshipStandard> ReadStandardsFromFile()
        {
            var apprenticeshipStandardList = new List<ApprenticeshipStandard>();

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

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);

                    var status = row.GetCell(statusIndex).StringCellValue;

                    //What to do if a Job Profile has a standard that's not approved? -- Will not associate at the moment
                    if (status.Equals("Approved for delivery", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var name = this.CleanseStandardName(row.GetCell(nameIndex).StringCellValue);
                        var reference = row.GetCell(referenceIndex).StringCellValue;
                        var level = row.GetCell(levelIndex).CellType == CellType.Numeric ? row.GetCell(levelIndex).NumericCellValue : 0;
                        var maximumFunding = row.GetCell(maximumFundingIndex).CellType == CellType.Numeric ? row.GetCell(maximumFundingIndex).NumericCellValue : 0;
                        var typicalDuration = row.GetCell(typicalDurationIndex).NumericCellValue;
                        var larsCode = row.GetCell(larsCodeIndex).CellType == CellType.Numeric ? row.GetCell(larsCodeIndex).NumericCellValue : 0;
                        var route = row.GetCell(routeIndex).StringCellValue;

                        var apprenticeshipStandard = new ApprenticeshipStandard
                        {
                            Name = name,
                            Reference = reference,
                            Level = Convert.ToInt32(level),
                            MaximumFunding = Convert.ToInt32(maximumFunding),
                            Duration = Convert.ToInt32(typicalDuration),
                            LARSCode = Convert.ToInt32(larsCode),
                            Route = route.Split(',').Select(z => $"{char.ToUpper(z.Trim()[0]) + z.Trim().Substring(1)}").ToArray(),
                            Type = "Standard"
                        };

                        apprenticeshipStandardList.Add(apprenticeshipStandard);
                    }
                }
            }

            BuildFrameworks(apprenticeshipStandardList);

            return apprenticeshipStandardList;
        }

        /// <summary>
        /// Add frameworks into the Standards collection
        /// </summary>
        /// <param name="apprenticeshipStandardList"></param>
        private static void BuildFrameworks(List<ApprenticeshipStandard> apprenticeshipStandardList)
        {
            //Load frameworks where standards are empty
            using (var reader = new StreamReader(@"SeedData\job_profiles.xlsx"))
            {
                var workbook = new XSSFWorkbook(reader.BaseStream);
                var sheet = workbook.GetSheet("JobProfileSoc");

                var apprenticeshipStandardsIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "apprenticeshipstandards").ColumnIndex;
                var apprenticeshipFrameworksIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "apprenticeshipframeworks").ColumnIndex;

                var jobProfileStandards = new Dictionary<string, string[]>();

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    var standards = row.GetCell(apprenticeshipStandardsIndex).StringCellValue;
                    var frameworks = row.GetCell(apprenticeshipFrameworksIndex).StringCellValue;

                    if (string.IsNullOrWhiteSpace(standards) && !string.IsNullOrWhiteSpace(frameworks))
                    {
                        var splitFrameworks = frameworks.Split(",");

                        foreach (var framework in splitFrameworks)
                        {
                            var apprenticeshipStandard = new ApprenticeshipStandard
                            {
                                Name = framework,
                                Type = "Framework"
                            };

                            //Only add frameworks that don't already exist in the list
                            if (!apprenticeshipStandardList.Any(x => x.Type == "Framework" && x.Name == framework))
                            {
                                apprenticeshipStandardList.Add(apprenticeshipStandard);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Iterates Standards in Job Profiles and associates to an Apprenticeship Standard
        /// </summary>
        /// <param name="jobProfiles"></param>
        private void AssignStandardsToJobProfiles(IEnumerable<JobProfileContentItem> jobProfiles)
        {
            using (var reader = new StreamReader(@"SeedData\job_profiles.xlsx"))
            {
                var workbook = new XSSFWorkbook(reader.BaseStream);
                var sheet = workbook.GetSheet("JobProfileSoc");
                var apprenticeshipStandardsIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "apprenticeshipstandards").ColumnIndex;
                var apprenticeshipFrameworkIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "apprenticeshipframeworks").ColumnIndex;
                var descriptionIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "Description").ColumnIndex;

                var jobProfileStandards = new Dictionary<string, string[]>();

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    var jobProfileDescription = row.GetCell(descriptionIndex).StringCellValue;
                    var standards = row.GetCell(apprenticeshipStandardsIndex).StringCellValue;
                    var frameworks = row.GetCell(apprenticeshipFrameworkIndex).StringCellValue;

                    var cleansedStandards = CleanseStandardName(standards);
                    var splitStandards = cleansedStandards.Split(',').ToList();

                    //Standards take precedent over frameworks, only add frameworks if there are no standards
                    if (!splitStandards.Any() && !string.IsNullOrEmpty(frameworks))
                    {
                        splitStandards.AddRange(frameworks.Split(','));
                    }

                    jobProfileStandards.Add(jobProfileDescription, splitStandards.ToArray());
                }

                foreach (var jobProfile in jobProfiles)
                {
                    jobProfile.EponymousPart.ApprenticeshipStandards = new ContentPicker();

                    var jobProfileStandardContentIds = new List<string>();

                    var applicableJobProfileStandards = jobProfileStandards.ContainsKey(jobProfile.TitlePart.Title) ? jobProfileStandards[jobProfile.TitlePart.Title] : null;

                    if (applicableJobProfileStandards != null && applicableJobProfileStandards.Any())
                    {
                        foreach (var standard in applicableJobProfileStandards)
                        {
                            if (!string.IsNullOrWhiteSpace(standard))
                            {
                                var standardContentId = _standardsDictionary.FirstOrDefault(x => x.Key.Equals(ReplaceStandardName(standard), StringComparison.InvariantCultureIgnoreCase)).Value;

                                //Will only be null for non-approved standards (Currently only Nursing Associate)
                                if (standardContentId != null)
                                {
                                    jobProfileStandardContentIds.Add(standardContentId);
                                }
                            }
                        }
                        
                        if (jobProfileStandardContentIds.Any())
                        {
                            //TODO : check for duplicates to report?
                            jobProfile.EponymousPart.ApprenticeshipStandards.ContentItemIds = jobProfileStandardContentIds.Distinct();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Cleanse a Standard Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string CleanseStandardName(string name)
        {
            //Cleanse standard names to match Job Profiles
            name = Regex.Replace(name, @"\s+\([d|D|L|l|i|I|c|C|b|B|r|R].*?\)", "");
            name = name.Replace(" / ", " ");
            name = name.Replace("/", " ");
            name = name.Replace("  ", "");
            name = name.Trim();
            return name;
        }

        /// <summary>
        /// Replace an NCS Standard with the IFA equivelant
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string ReplaceStandardName(string name)
        {
            name = name.Trim();
            var mappedResult = _replacementsDictionary.ContainsKey(name) ? _replacementsDictionary[name] : name;
            return mappedResult;
        }

        /// <summary>
        /// Builds a dictionary of replacements NCS -> IFA
        /// </summary>
        private void BuildReplacementsDictionary()
        {
            //Note: Nursing Associate will not map as it has now been retired
            _replacementsDictionary.Add("Beauty and Makeup Consultant", "Beauty and Make up Consultant");
            _replacementsDictionary.Add("Beauty therapy", "Beauty Therapist");
            _replacementsDictionary.Add("Accounts-finance assistant", "Accounts finance assistant");
            _replacementsDictionary.Add("Passenger Transport Driver bus coach and rail", "Passenger transport driver - bus, coach and tram");
            _replacementsDictionary.Add("Retail Leadership", "Retail leadership degree apprenticeship");
            _replacementsDictionary.Add("Children Young People and Families Manager", "Children, young people and families manager");
            _replacementsDictionary.Add("Floor Layer", "Floorlayer");
            _replacementsDictionary.Add("Senior Chef Production Cooking", "Senior Production Chef");
            _replacementsDictionary.Add("Registered Nurse 2010", "Registered nurse - degree (NMC 2010)");
            _replacementsDictionary.Add("Registered Nurse 2018", "Registered nurse degree (NMC 2018)");
            _replacementsDictionary.Add("Cultural Learning and Participation Officers", "Cultural Learning and Participation Officer");
            _replacementsDictionary.Add("Electrical Electronic Product Service and Installation Engineer", "Electrical, electronic product service and installation engineer");
            _replacementsDictionary.Add("Leisure and Entertainment Maintenance Engineering Technician", "Leisure and entertainment engineering technician");
            _replacementsDictionary.Add("Metal Casting Foundry and Patternmaking Technician", "Metal casting, foundry and patternmaking technician");
            _replacementsDictionary.Add("Ambulance Support Worker (Emergency Urgent and Non-Urgent)", "Ambulance support worker (emergency, urgent and non-urgent)");
            _replacementsDictionary.Add("Leather craftworker", "Leather craftsperson");
            _replacementsDictionary.Add("Senior Leader Master's Degree Apprenticeship", "Senior leader");
            _replacementsDictionary.Add("Geospatial Mapping and Science Degree", "Geospatial mapping and science specialist");
            _replacementsDictionary.Add("Library info and archive assistant", "Library, information and archive services assistant");
            _replacementsDictionary.Add("Chartered Manager Degree Apprenticeship", "Chartered Manager");
            _replacementsDictionary.Add("Gas Engineering", "Gas engineering operative");
            _replacementsDictionary.Add("Safety Health and Environment Technician", "Safety, health and environment technician");
            _replacementsDictionary.Add("Revenue and Welfare Benefits Practitioner", "Revenues and welfare benefits practitioner");
            _replacementsDictionary.Add("Nail Service Technician", "Nail services technician");
            _replacementsDictionary.Add("Children Young People and Families Practitioner", "Children, young people and families practitioner");
            _replacementsDictionary.Add("Systems Engineering Masters Level", "Systems engineer");
            _replacementsDictionary.Add("Nursing associate NMC2018", "Nursing associate (NMC 2018)");
            _replacementsDictionary.Add("Public Relations Assistant", "Public relations and communications assistant");
            _replacementsDictionary.Add("Assistant buyer-assistant mechandiser", "Assistant buyer Assistant merchandiser");
            _replacementsDictionary.Add("Engineering Construction Erector-Rigger", "Engineering construction erector rigger");
            _replacementsDictionary.Add("Construction Steel Fixer", "Steel fixer");
            _replacementsDictionary.Add("Supply Chain Leadership", "Supply chain leadership professional");
            _replacementsDictionary.Add("Supply Chain Practitioner", "Supply chain practitioner (fast moving consumer good) [previously Operator Manager]");
            _replacementsDictionary.Add("Learning and Development ConsultantBusiness Partner", "Learning and development consultant business partner");
            _replacementsDictionary.Add("Veterinary nursing", "Veterinary nurse");
            _replacementsDictionary.Add("Welding", "Plate welder");
        }
    }
}

