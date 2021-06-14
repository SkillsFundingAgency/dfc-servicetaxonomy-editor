using System;
using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.Spreadsheet.JobProfile;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles.Importers.Spreadsheets
{
    public class JobProfileSpreadsheetImporter
    {
        // The CollegeLink dictionary of imported data in key/value format where the value is row model for the given spreadsheet
        public IReadOnlyDictionary<string, JobProfileSpreadsheetRowModel> JobProfileDictionary { get; private set; }

        // The name of the Spreadsheet to import
        private const string SpreadsheetName = "JobProfile";

        public IReadOnlyDictionary<string, JobProfileSpreadsheetRowModel> Import(XSSFWorkbook spreadsheetWorkbook)
        {
            // Initialise the dictionary that will hold the Spreadsheet row/columns model
            var spreadsheetDictionary = new Dictionary<string, JobProfileSpreadsheetRowModel>();

            // Get a reference to the Spreadsheet in the workbook
            var spreadsheet = spreadsheetWorkbook.GetSheet(SpreadsheetName);

            // Initialise the spreadsheet column index model to hold the columns indexes
            var spreadsheetColumnIndexModel = new JobProfileSpreadsheetColumnIndexModel();

            // Initialise a spreadsheet row model so that we can use the column names to lookup the column indexes
            var spreadsheetRowModel = new JobProfileSpreadsheetRowModel();

            // Initialise the column index values
            InitialiseTheSpreadsheetColumnIndexes(spreadsheet, spreadsheetColumnIndexModel, spreadsheetRowModel);

            // Iterate through the rows in the spreadsheet to get the row data
            for (int i = 1; i <= spreadsheet.LastRowNum; ++i)
            {
                // Get the row data
                var spreadsheetRow = spreadsheet.GetRow(i);

                // Get the row data (first time through we are discarding the socCodesSpreadsheetRowModel model used above which is used to get the column indexes)
                spreadsheetRowModel = GetTheSpreadsheetRowData(spreadsheetRow, spreadsheetColumnIndexModel);

                // Ignore blank rows
                if (spreadsheetRowModel != null)
                {
                    // Add the Spreadsheet to the dictionary
                    spreadsheetDictionary.Add(spreadsheetRowModel.ItemDefaultUrl.TrimStart('/'), spreadsheetRowModel);
                }
            }

            // Create a read-only version of the dictionary for public acsess to stop inadvertent changes to the data
            IReadOnlyDictionary<string, JobProfileSpreadsheetRowModel> readonlySpreadsheet = spreadsheetDictionary;

            return readonlySpreadsheet;
        }

        private void InitialiseTheSpreadsheetColumnIndexes(
            NPOI.SS.UserModel.ISheet spreadsheet,
            JobProfileSpreadsheetColumnIndexModel spreadsheetColumnIndexModel,
            JobProfileSpreadsheetRowModel spreadsheetRowModel)
        {
            // SystemParentIdColumnIndex
            spreadsheetColumnIndexModel.SystemParentIdColumnIndex = spreadsheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.SystemParentId))
                .ColumnIndex;

            // IncludeInSitemapColumnIndex
            spreadsheetColumnIndexModel.IncludeInSitemapColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.IncludeInSitemap))
                  .ColumnIndex;

            // IdColumnIndex
            spreadsheetColumnIndexModel.IdColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Id))
                  .ColumnIndex;

            // DateCreatedColumnIndex
            spreadsheetColumnIndexModel.DateCreatedColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.DateCreated))
                  .ColumnIndex;

            // ItemDefaultUrlColumnIndex
            spreadsheetColumnIndexModel.ItemDefaultUrlColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.ItemDefaultUrl))
                  .ColumnIndex;

            // UrlNameColumnIndex
            spreadsheetColumnIndexModel.UrlNameColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.UrlName))
                  .ColumnIndex;

            // PublicationDateColumnIndex
            spreadsheetColumnIndexModel.PublicationDateColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.PublicationDate))
                  .ColumnIndex;

            // IsLMISalaryFeedOverriden
            spreadsheetColumnIndexModel.IsLMISalaryFeedOverridenColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.IsLMISalaryFeedOverriden))
                  .ColumnIndex;

            // MinimumHours
            spreadsheetColumnIndexModel.MinimumHoursColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.MinimumHours))
                  .ColumnIndex;

            // WhatYouWillDo
            spreadsheetColumnIndexModel.WhatYouWillDoColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.WhatYouWillDo))
                  .ColumnIndex;

            // HiddenAlternativeTitle
            spreadsheetColumnIndexModel.HiddenAlternativeTitleColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.HiddenAlternativeTitle))
                  .ColumnIndex;

            // IsImported
            spreadsheetColumnIndexModel.IsImportedColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.IsImported))
                  .ColumnIndex;

            // DynamicTitlePrefix
            spreadsheetColumnIndexModel.DynamicTitlePrefixColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.DynamicTitlePrefix))
                  .ColumnIndex;

            // WYDIntroduction
            spreadsheetColumnIndexModel.WYDIntroductionColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.WYDIntroduction))
                  .ColumnIndex;

            // WorkingHoursDetails
            spreadsheetColumnIndexModel.WorkingHoursDetailsColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.WorkingHoursDetails))
                  .ColumnIndex;

            // CareerTips
            spreadsheetColumnIndexModel.CareerTipsColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.CareerTips))
                  .ColumnIndex;

            // Salary
            spreadsheetColumnIndexModel.SalaryColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Salary))
                  .ColumnIndex;

            // ApprenticeshipEntryRequirements
            spreadsheetColumnIndexModel.ApprenticeshipEntryRequirementsColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.ApprenticeshipEntryRequirements))
                  .ColumnIndex;

            // Volunteering
            spreadsheetColumnIndexModel.VolunteeringColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Volunteering))
                  .ColumnIndex;

            // CollegeFurtherRouteInfo
            spreadsheetColumnIndexModel.CollegeFurtherRouteInfoColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.CollegeFurtherRouteInfo))
                  .ColumnIndex;

            // SalaryRange
            spreadsheetColumnIndexModel.SalaryRangeColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.SalaryRange))
                  .ColumnIndex;

            // WorkingPatternDetails
            spreadsheetColumnIndexModel.WorkingPatternDetailsColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.WorkingPatternDetails))
                  .ColumnIndex;

            // Overview
            spreadsheetColumnIndexModel.OverviewColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Overview))
                  .ColumnIndex;

            // BAUSystemOverrideUrl
            spreadsheetColumnIndexModel.BAUSystemOverrideUrlColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.BAUSystemOverrideUrl))
                  .ColumnIndex;

            // CollegeRelevantSubjects
            spreadsheetColumnIndexModel.CollegeRelevantSubjectsColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.CollegeRelevantSubjects))
                  .ColumnIndex;

            // JobProfileCategories
            spreadsheetColumnIndexModel.JobProfileCategoriesColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.JobProfileCategories))
                  .ColumnIndex;

            // IsWYDCadReady
            spreadsheetColumnIndexModel.IsWYDCadReadyColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.IsWYDCadReady))
                  .ColumnIndex;

            // ProfessionalAndIndustryBodies
            spreadsheetColumnIndexModel.ProfessionalAndIndustryBodiesColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.ProfessionalAndIndustryBodies))
                  .ColumnIndex;

            // UniversityFurtherRouteInfo
            spreadsheetColumnIndexModel.UniversityFurtherRouteInfoColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.UniversityFurtherRouteInfo))
                  .ColumnIndex;

            // ApprenticeshipFurtherRouteInfo
            spreadsheetColumnIndexModel.ApprenticeshipFurtherRouteInfoColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.ApprenticeshipFurtherRouteInfo))
                  .ColumnIndex;

            // OtherRoutes
            spreadsheetColumnIndexModel.OtherRoutesColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.OtherRoutes))
                  .ColumnIndex;

            // CourseKeywords
            spreadsheetColumnIndexModel.CourseKeywordsColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.CourseKeywords))
                  .ColumnIndex;

            // ApprenticeshipRelevantSubjects
            spreadsheetColumnIndexModel.ApprenticeshipRelevantSubjectsColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.ApprenticeshipRelevantSubjects))
                  .ColumnIndex;

            // CollegeEntryRequirements
            spreadsheetColumnIndexModel.CollegeEntryRequirementsColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.CollegeEntryRequirements))
                  .ColumnIndex;

            // UniversityRelevantSubjects
            spreadsheetColumnIndexModel.UniversityRelevantSubjectsColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.UniversityRelevantSubjects))
                  .ColumnIndex;

            // WorkingPattern
            spreadsheetColumnIndexModel.WorkingPatternColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.WorkingPattern))
                  .ColumnIndex;

            // EntryRoutes
            spreadsheetColumnIndexModel.EntryRoutesColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.EntryRoutes))
                  .ColumnIndex;

            // CareerPathAndProgression
            spreadsheetColumnIndexModel.CareerPathAndProgressionColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.CareerPathAndProgression))
                  .ColumnIndex;

            // DoesNotExistInBAU
            spreadsheetColumnIndexModel.DoesNotExistInBAUColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.DoesNotExistInBAU))
                  .ColumnIndex;

            // OtherRequirements
            spreadsheetColumnIndexModel.OtherRequirementsColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.OtherRequirements))
                  .ColumnIndex;

            // WYDDayToDayTasks
            spreadsheetColumnIndexModel.WYDDayToDayTasksColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.WYDDayToDayTasks))
                  .ColumnIndex;

            // DigitalSkillsLevel
            spreadsheetColumnIndexModel.DigitalSkillsLevelColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.DigitalSkillsLevel))
                  .ColumnIndex;

            // JobProfileSpecialism
            spreadsheetColumnIndexModel.JobProfileSpecialismColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.JobProfileSpecialism))
                  .ColumnIndex;

            // FurtherInformation
            spreadsheetColumnIndexModel.FurtherInformationColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.FurtherInformation))
                  .ColumnIndex;

            // Title
            spreadsheetColumnIndexModel.TitleColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Title))
                  .ColumnIndex;

            // HowToBecome
            spreadsheetColumnIndexModel.HowToBecomeColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.HowToBecome))
                  .ColumnIndex;

            // DirectApplication
            spreadsheetColumnIndexModel.DirectApplicationColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.DirectApplication))
                  .ColumnIndex;

            // MaximumHours
            spreadsheetColumnIndexModel.MaximumHoursColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.MaximumHours))
                  .ColumnIndex;

            // WorkingHoursPatternsAndEnvironment
            spreadsheetColumnIndexModel.WorkingHoursPatternsAndEnvironmentColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.WorkingHoursPatternsAndEnvironment))
                  .ColumnIndex;

            // SalaryExperienced
            spreadsheetColumnIndexModel.SalaryExperiencedColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.SalaryExperienced))
                  .ColumnIndex;

            // Skills
            spreadsheetColumnIndexModel.SkillsColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Skills))
                  .ColumnIndex;

            // UniversityEntryRequirements
            spreadsheetColumnIndexModel.UniversityEntryRequirementsColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.UniversityEntryRequirements))
                  .ColumnIndex;

            // IsHTBCaDReady
            spreadsheetColumnIndexModel.IsHTBCaDReadyColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.IsHTBCaDReady))
                  .ColumnIndex;

            // Work
            spreadsheetColumnIndexModel.WorkColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.Work))
                  .ColumnIndex;

            // SalaryStarter
            spreadsheetColumnIndexModel.SalaryStarterColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.SalaryStarter))
                  .ColumnIndex;

            // WidgetContentTitle
            spreadsheetColumnIndexModel.WidgetContentTitleColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.WidgetContentTitle))
                  .ColumnIndex;

            // AlternativeTitle
            spreadsheetColumnIndexModel.AlternativeTitleColumnIndex = spreadsheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(spreadsheetRowModel.AlternativeTitle))
                  .ColumnIndex;
        }

        private JobProfileSpreadsheetRowModel GetTheSpreadsheetRowData(
            NPOI.SS.UserModel.IRow spreadsheetRow,
            JobProfileSpreadsheetColumnIndexModel spreadsheetColumnIndexModel)
        {
            JobProfileSpreadsheetRowModel spreadsheetRowModel = null;

            if (spreadsheetRow != null)
            {
                spreadsheetRowModel = new JobProfileSpreadsheetRowModel();

                // SystemParentId
                NPOI.SS.UserModel.ICell systemParentId = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.SystemParentIdColumnIndex);
                if (systemParentId != null)
                {
                    spreadsheetRowModel.SystemParentId = systemParentId.StringCellValue;
                }

                // IncludeInSitemap
                NPOI.SS.UserModel.ICell includeInSitemap = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.IncludeInSitemapColumnIndex);
                if (includeInSitemap != null)
                {
                    spreadsheetRowModel.IncludeInSitemap = Convert.ToBoolean(includeInSitemap.StringCellValue);
                }

                // Id
                NPOI.SS.UserModel.ICell id = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.IdColumnIndex);
                if (id != null)
                {
                    spreadsheetRowModel.Id = id.StringCellValue;
                }

                // DateCreated
                NPOI.SS.UserModel.ICell dateCreated = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.DateCreatedColumnIndex);
                if (dateCreated != null)
                {
                    spreadsheetRowModel.DateCreated = Convert.ToDateTime(dateCreated.StringCellValue);
                }

                // ItemDefaultUrl
                NPOI.SS.UserModel.ICell itemDefaultUrl = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.ItemDefaultUrlColumnIndex);
                if (itemDefaultUrl != null)
                {
                    spreadsheetRowModel.ItemDefaultUrl = itemDefaultUrl.StringCellValue;
                }

                // UrlName
                NPOI.SS.UserModel.ICell urlName = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.UrlNameColumnIndex);
                if (urlName != null)
                {
                    spreadsheetRowModel.UrlName = urlName.StringCellValue;
                }

                // PublicationDate
                NPOI.SS.UserModel.ICell publicationDate = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.PublicationDateColumnIndex);
                if (publicationDate != null)
                {
                    spreadsheetRowModel.PublicationDate = Convert.ToDateTime(publicationDate.StringCellValue);
                }

                // IsLMISalaryFeedOverriden
                NPOI.SS.UserModel.ICell isLMISalaryFeedOverriden = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.IsLMISalaryFeedOverridenColumnIndex);
                if (isLMISalaryFeedOverriden != null)
                {
                    spreadsheetRowModel.IsLMISalaryFeedOverriden = Convert.ToBoolean(isLMISalaryFeedOverriden.StringCellValue);
                }

                // MinimumHours
                NPOI.SS.UserModel.ICell minimumHours = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.MinimumHoursColumnIndex);
                if (minimumHours != null)
                {
                    spreadsheetRowModel.MinimumHours = Convert.ToDecimal(minimumHours.StringCellValue);
                }

                // WhatYouWillDo
                NPOI.SS.UserModel.ICell whatYouWillDo = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.WhatYouWillDoColumnIndex);
                if (whatYouWillDo != null)
                {
                    spreadsheetRowModel.WhatYouWillDo = whatYouWillDo.StringCellValue;
                }

                // HiddenAlternativeTitle
                NPOI.SS.UserModel.ICell hiddenAlternativeTitle = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.HiddenAlternativeTitleColumnIndex);
                if (hiddenAlternativeTitle != null)
                {
                    spreadsheetRowModel.HiddenAlternativeTitle = hiddenAlternativeTitle.StringCellValue;
                }

                // IsImported
                NPOI.SS.UserModel.ICell isImported = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.IsImportedColumnIndex);
                if (isImported != null)
                {
                    spreadsheetRowModel.IsImported = Convert.ToBoolean(isImported.StringCellValue);
                }

                // DynamicTitlePrefix
                NPOI.SS.UserModel.ICell dynamicTitlePrefix = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.DynamicTitlePrefixColumnIndex);
                if (dynamicTitlePrefix != null)
                {
                    spreadsheetRowModel.DynamicTitlePrefix = dynamicTitlePrefix.StringCellValue;
                }

                // WYDIntroduction
                NPOI.SS.UserModel.ICell wYDIntroduction = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.WYDIntroductionColumnIndex);
                if (wYDIntroduction != null)
                {
                    spreadsheetRowModel.WYDIntroduction = wYDIntroduction.StringCellValue;
                }

                // WorkingHoursDetails
                NPOI.SS.UserModel.ICell workingHoursDetails = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.WorkingHoursDetailsColumnIndex);
                if (workingHoursDetails != null)
                {
                    spreadsheetRowModel.WorkingHoursDetails = workingHoursDetails.StringCellValue;
                }

                // CareerTips
                NPOI.SS.UserModel.ICell careerTips = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.CareerTipsColumnIndex);
                if (careerTips != null)
                {
                    spreadsheetRowModel.CareerTips = careerTips.StringCellValue;
                }

                // Salary
                NPOI.SS.UserModel.ICell salary = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.SalaryColumnIndex);
                if (salary != null)
                {
                    spreadsheetRowModel.Salary = salary.StringCellValue;
                }

                // ApprenticeshipEntryRequirements
                NPOI.SS.UserModel.ICell ApprenticeshipEntryRequirements = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.ApprenticeshipEntryRequirementsColumnIndex);
                if (ApprenticeshipEntryRequirements != null)
                {
                    spreadsheetRowModel.ApprenticeshipEntryRequirements = ApprenticeshipEntryRequirements.StringCellValue;
                }

                // Volunteering
                NPOI.SS.UserModel.ICell volunteering = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.VolunteeringColumnIndex);
                if (volunteering != null)
                {
                    spreadsheetRowModel.Volunteering = volunteering.StringCellValue;
                }

                // CollegeFurtherRouteInfo
                NPOI.SS.UserModel.ICell collegeFurtherRouteInfo = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.CollegeFurtherRouteInfoColumnIndex);
                if (collegeFurtherRouteInfo != null)
                {
                    spreadsheetRowModel.CollegeFurtherRouteInfo = collegeFurtherRouteInfo.StringCellValue;
                }

                // SalaryRange
                NPOI.SS.UserModel.ICell salaryRange = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.SalaryRangeColumnIndex);
                if (salaryRange != null)
                {
                    spreadsheetRowModel.SalaryRange = salaryRange.StringCellValue;
                }

                // WorkingPatternDetails
                NPOI.SS.UserModel.ICell workingPatternDetails = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.WorkingPatternDetailsColumnIndex);
                if (workingPatternDetails != null)
                {
                    spreadsheetRowModel.WorkingPatternDetails = workingPatternDetails.StringCellValue;
                }

                // Overview
                NPOI.SS.UserModel.ICell overview = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.OverviewColumnIndex);
                if (overview != null)
                {
                    spreadsheetRowModel.Overview = overview.StringCellValue;
                }

                // BAUSystemOverrideUrl
                NPOI.SS.UserModel.ICell bAUSystemOverrideUrl = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.BAUSystemOverrideUrlColumnIndex);
                if (bAUSystemOverrideUrl != null)
                {
                    spreadsheetRowModel.BAUSystemOverrideUrl = bAUSystemOverrideUrl.StringCellValue;
                }

                // CollegeRelevantSubjects
                NPOI.SS.UserModel.ICell collegeRelevantSubjects = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.CollegeRelevantSubjectsColumnIndex);
                if (collegeRelevantSubjects != null)
                {
                    spreadsheetRowModel.CollegeRelevantSubjects = collegeRelevantSubjects.StringCellValue;
                }

                // JobProfileCategories
                NPOI.SS.UserModel.ICell jobProfileCategories = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.JobProfileCategoriesColumnIndex);
                if (jobProfileCategories != null)
                {
                    spreadsheetRowModel.JobProfileCategories = jobProfileCategories.StringCellValue;
                }

                // IsWYDCadReady
                NPOI.SS.UserModel.ICell isWYDCadReady = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.IsWYDCadReadyColumnIndex);
                if (isWYDCadReady != null)
                {
                    spreadsheetRowModel.IsWYDCadReady = Convert.ToBoolean(isWYDCadReady.StringCellValue);
                }

                // ProfessionalAndIndustryBodies
                NPOI.SS.UserModel.ICell professionalAndIndustryBodies = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.ProfessionalAndIndustryBodiesColumnIndex);
                if (professionalAndIndustryBodies != null)
                {
                    spreadsheetRowModel.ProfessionalAndIndustryBodies = professionalAndIndustryBodies.StringCellValue;
                }

                // UniversityFurtherRouteInfo
                NPOI.SS.UserModel.ICell universityFurtherRouteInfo = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.UniversityFurtherRouteInfoColumnIndex);
                if (universityFurtherRouteInfo != null)
                {
                    spreadsheetRowModel.UniversityFurtherRouteInfo = universityFurtherRouteInfo.StringCellValue;
                }

                // ApprenticeshipFurtherRouteInfo
                NPOI.SS.UserModel.ICell apprenticeshipFurtherRouteInfo = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.ApprenticeshipFurtherRouteInfoColumnIndex);
                if (apprenticeshipFurtherRouteInfo != null)
                {
                    spreadsheetRowModel.ApprenticeshipFurtherRouteInfo = apprenticeshipFurtherRouteInfo.StringCellValue;
                }

                // OtherRoutes
                NPOI.SS.UserModel.ICell otherRoutes = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.OtherRoutesColumnIndex);
                if (otherRoutes != null)
                {
                    spreadsheetRowModel.OtherRoutes = otherRoutes.StringCellValue;
                }

                // CourseKeywords
                NPOI.SS.UserModel.ICell courseKeywords = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.CourseKeywordsColumnIndex);
                if (courseKeywords != null)
                {
                    spreadsheetRowModel.CourseKeywords = courseKeywords.StringCellValue;
                }

                // ApprenticeshipRelevantSubjects
                NPOI.SS.UserModel.ICell apprenticeshipRelevantSubjects = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.ApprenticeshipRelevantSubjectsColumnIndex);
                if (apprenticeshipRelevantSubjects != null)
                {
                    spreadsheetRowModel.ApprenticeshipRelevantSubjects = apprenticeshipRelevantSubjects.StringCellValue;
                }

                // CollegeEntryRequirements
                NPOI.SS.UserModel.ICell collegeEntryRequirements = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.CollegeEntryRequirementsColumnIndex);
                if (collegeEntryRequirements != null)
                {
                    spreadsheetRowModel.CollegeEntryRequirements = collegeEntryRequirements.StringCellValue;
                }

                // UniversityRelevantSubjects
                NPOI.SS.UserModel.ICell universityRelevantSubjects = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.UniversityRelevantSubjectsColumnIndex);
                if (universityRelevantSubjects != null)
                {
                    spreadsheetRowModel.UniversityRelevantSubjects = universityRelevantSubjects.StringCellValue;
                }

                // WorkingPattern
                NPOI.SS.UserModel.ICell workingPattern = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.WorkingPatternColumnIndex);
                if (workingPattern != null)
                {
                    spreadsheetRowModel.WorkingPattern = workingPattern.StringCellValue;
                }

                // EntryRoutes
                NPOI.SS.UserModel.ICell entryRoutes = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.EntryRoutesColumnIndex);
                if (entryRoutes != null)
                {
                    spreadsheetRowModel.EntryRoutes = entryRoutes.StringCellValue;
                }

                // CareerPathAndProgression
                NPOI.SS.UserModel.ICell careerPathAndProgression = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.CareerPathAndProgressionColumnIndex);
                if (careerPathAndProgression != null)
                {
                    spreadsheetRowModel.CareerPathAndProgression = careerPathAndProgression.StringCellValue;
                }

                // DoesNotExistInBAU
                NPOI.SS.UserModel.ICell doesNotExistInBAU = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.DoesNotExistInBAUColumnIndex);
                if (doesNotExistInBAU != null)
                {
                    spreadsheetRowModel.DoesNotExistInBAU = Convert.ToBoolean(doesNotExistInBAU.StringCellValue);
                }

                // OtherRequirements
                NPOI.SS.UserModel.ICell otherRequirements = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.OtherRequirementsColumnIndex);
                if (otherRequirements != null)
                {
                    spreadsheetRowModel.OtherRequirements = otherRequirements.StringCellValue;
                }

                // WYDDayToDayTasks
                NPOI.SS.UserModel.ICell wYDDayToDayTasks = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.WYDDayToDayTasksColumnIndex);
                if (wYDDayToDayTasks != null)
                {
                    spreadsheetRowModel.WYDDayToDayTasks = wYDDayToDayTasks.StringCellValue;
                }

                // DigitalSkillsLevel
                NPOI.SS.UserModel.ICell digitalSkillsLevel = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.DigitalSkillsLevelColumnIndex);
                if (digitalSkillsLevel != null)
                {
                    spreadsheetRowModel.DigitalSkillsLevel = digitalSkillsLevel.StringCellValue;
                }

                // JobProfileSpecialism
                NPOI.SS.UserModel.ICell jobProfileSpecialism = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.JobProfileSpecialismColumnIndex);
                if (jobProfileSpecialism != null)
                {
                    spreadsheetRowModel.JobProfileSpecialism = jobProfileSpecialism.StringCellValue;
                }

                // FurtherInformation
                NPOI.SS.UserModel.ICell furtherInformation = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.FurtherInformationColumnIndex);
                if (furtherInformation != null)
                {
                    spreadsheetRowModel.FurtherInformation = furtherInformation.StringCellValue;
                }

                // Title
                NPOI.SS.UserModel.ICell title = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.TitleColumnIndex);
                if (title != null)
                {
                    spreadsheetRowModel.Title = title.StringCellValue;
                }

                // HowToBecome
                NPOI.SS.UserModel.ICell howToBecome = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.HowToBecomeColumnIndex);
                if (howToBecome != null)
                {
                    spreadsheetRowModel.HowToBecome = howToBecome.StringCellValue;
                }

                // DirectApplication
                NPOI.SS.UserModel.ICell directApplication = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.DirectApplicationColumnIndex);
                if (directApplication != null)
                {
                    spreadsheetRowModel.DirectApplication = directApplication.StringCellValue;
                }

                // MaximumHours
                NPOI.SS.UserModel.ICell MaximumHours = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.MaximumHoursColumnIndex);
                if (MaximumHours != null)
                {
                    spreadsheetRowModel.MaximumHours = Convert.ToDecimal(MaximumHours.StringCellValue);
                }

                // WorkingHoursPatternsAndEnvironment
                NPOI.SS.UserModel.ICell workingHoursPatternsAndEnvironment = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.WorkingHoursPatternsAndEnvironmentColumnIndex);
                if (workingHoursPatternsAndEnvironment != null)
                {
                    spreadsheetRowModel.WorkingHoursPatternsAndEnvironment = workingHoursPatternsAndEnvironment.StringCellValue;
                }

                // SalaryExperienced
                NPOI.SS.UserModel.ICell salaryExperienced = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.SalaryExperiencedColumnIndex);
                if (salaryExperienced != null)
                {
                    spreadsheetRowModel.SalaryExperienced = Convert.ToInt64(salaryExperienced.StringCellValue);
                }

                // Skills
                NPOI.SS.UserModel.ICell skills = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.SkillsColumnIndex);
                if (skills != null)
                {
                    spreadsheetRowModel.Skills = skills.StringCellValue;
                }

                // UniversityEntryRequirements
                NPOI.SS.UserModel.ICell universityEntryRequirements = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.UniversityEntryRequirementsColumnIndex);
                if (universityEntryRequirements != null)
                {
                    spreadsheetRowModel.UniversityEntryRequirements = universityEntryRequirements.StringCellValue;
                }

                // IsHTBCaDReady
                NPOI.SS.UserModel.ICell isHTBCaDReady = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.IsHTBCaDReadyColumnIndex);
                if (isHTBCaDReady != null)
                {
                    spreadsheetRowModel.IsHTBCaDReady = Convert.ToBoolean(isHTBCaDReady.StringCellValue);
                }

                // Work
                NPOI.SS.UserModel.ICell work = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.WorkColumnIndex);
                if (work != null)
                {
                    spreadsheetRowModel.Work = work.StringCellValue;
                }

                // SalaryStarter
                NPOI.SS.UserModel.ICell salaryStarter = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.SalaryStarterColumnIndex);
                if (salaryStarter != null)
                {
                    spreadsheetRowModel.SalaryStarter = Convert.ToInt64(salaryStarter.StringCellValue);
                }

                // WidgetContentTitle
                NPOI.SS.UserModel.ICell widgetContentTitle = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.WidgetContentTitleColumnIndex);
                if (widgetContentTitle != null)
                {
                    spreadsheetRowModel.WidgetContentTitle = widgetContentTitle.StringCellValue;
                }

                // AlternativeTitle
                NPOI.SS.UserModel.ICell alternativeTitle = spreadsheetRow.GetCell(spreadsheetColumnIndexModel.AlternativeTitleColumnIndex);
                if (alternativeTitle != null)
                {
                    spreadsheetRowModel.AlternativeTitle = alternativeTitle.StringCellValue;
                }

            }

            return spreadsheetRowModel;
        }
    }
}
