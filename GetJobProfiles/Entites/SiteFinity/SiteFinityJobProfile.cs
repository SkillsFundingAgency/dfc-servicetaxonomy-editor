using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using GetJobProfiles.Models.Containers;
using GetJobProfiles.Models.SiteFinity;
using GetJobProfiles.Models.SiteFinity.ApprencticeshipRequirementModels;
using GetJobProfiles.Models.SiteFinity.ApprenticeshipRequirementModels;
using GetJobProfiles.Models.SiteFinity.CollegeRequirementModels;
using GetJobProfiles.Models.SiteFinity.JobProfileModels;
using GetJobProfiles.Models.SiteFinity.SocSkillsMatrixModels;
using GetJobProfiles.Models.SiteFinity.UniversityRequirementModels;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles.Entites.Sitefinity
{
    public class SiteFinityJobProfile
    {
        #region Public read-only properties

        // Job Profile related entities from the ERD
        // Ordered top down left to right

        // Job Categories

        // Job Profiles
        public IReadOnlyDictionary<string, JobProfileWorksheetRowModel> JobProfileDictionary { get; private set; }

        // SOC Codes

        // ONet Occupational Codes

        // University Routes

        // College Routes

        // Apprenticeship Routes

        // Work Routes

        // Volunteering Routes

        // Direct Routes

        // Other Routes

        // Registration

        // Restrictions

        // Other Requirements

        // Digital Skills Levels

        // Working Environments

        // Working Locations

        // Working Uniforms

        // Apprenticeship Standards

        // ONet Skills

        // University Requirements
        public IReadOnlyDictionary<string, UniversityRequirementWorksheetRowModel> UniversityRequirementDictionary { get; private set; }

        // University Links

        // College Requirements
        public IReadOnlyDictionary<string, CollegeRequirementWorksheetRowModel> CollegeRequirementDictionary { get; private set; }

        // College Links

        // Apprenticeship Requirements
        public IReadOnlyDictionary<string, ApprenticeshipRequirementWorksheetRowModel> ApprenticeshipRequirementDictionary { get; private set; }

        // Apprenticeship Links

        // Apprenticeship Standard Routes

        // QCF Levels

        // SocSkillsMatrix
        public IReadOnlyDictionary<string, SocSkillsMatrixWorksheetRowModel> SocSkillsMatrixDictionary { get; private set; }

        #endregion Public read-only properties

        public SiteFinityJobProfile Import(SettingsModel settingsModel, XSSFWorkbook jobProfilesWorkbook)
        {
            // Reference the ERD for Job Profile
            // Import/build the associated reference entities ordered right to left, bottom to top

            // Import QCF Levels
            // TODO: Need source of QCF Levels

            // Import Apprenticeship Standard Routes

            // Import Apprenticeship Links

            // Import Apprenticeship Requirements
            ApprenticeshipRequirementDictionary = ImportApprenticeshipRequirementWorksheet(jobProfilesWorkbook);

            // Import College Links

            // Import College Requirements
            CollegeRequirementDictionary = ImportCollegeRequirementWorksheet(jobProfilesWorkbook);

            // Import University Links

            // Import University Requirements
            UniversityRequirementDictionary = ImportUniversityRequirementWorksheet(jobProfilesWorkbook);

            // Import ONet Skills

            // Import Apprenticeship Standards
            // TODO: Need source of Apprenticeship Standards

            // Import Working Uniforms

            // Import Working Locations

            // Import Working Environments

            // Import Digital Skills Levels

            // Import Other Requirements

            // Import Restrictions

            // Import Registrations

            // Import Other Routes (Specialist Training)

            // Import Direct Routes

            // Import Volunteering Routes

            // Import Work Routes

            // Import Apprenticeship Routes

            // Import College Routes

            // Import University Routes

            // Import ONet Occupational Codes

            // Import SOC Codes

            // Import the SocSkillsMatrix data from the exported Sitefinity spreadsheet
            SocSkillsMatrixDictionary = ImportSocSkillsMatrixWorksheet(jobProfilesWorkbook);

            // Import the Job Profile data from the exported SiteFinity spreadsheet
            JobProfileDictionary = ImportJobProfileWorksheet(jobProfilesWorkbook);

            return this;
        }

        #region SocSkillsMatrix Worksheet

        private IReadOnlyDictionary<string, SocSkillsMatrixWorksheetRowModel> ImportSocSkillsMatrixWorksheet(XSSFWorkbook jobProfilesWorkbook)
        {
            // Initialise the dictionary that will hold the SocSkillsMatrix worksheet row/columns model
            var socSkillsMatrixWorksheetDictionary = new Dictionary<string, SocSkillsMatrixWorksheetRowModel>();

            // Get a reference to the SocSkillsMatrix worksheet in the job profile workbook
            var socSkillsMatrixWorksheet = jobProfilesWorkbook.GetSheet("SocSkillsMatrix");

            // Initialise the model to hold the columns indexes
            var socSkillsMatrixWorksheetColumnIndexModel = new SocSkillsMatrixWorksheetColumnIndexModel();

            // Initialise a row model so that we can use the column names to get the column indexes
            var socSkillsMatrixWorksheetRowModel = new SocSkillsMatrixWorksheetRowModel();

            // Inialise the column index values
            InitialiseSocSkillsMatrixWorksheetColumnIndexes(socSkillsMatrixWorksheet, socSkillsMatrixWorksheetColumnIndexModel, socSkillsMatrixWorksheetRowModel);

            // iterate the rows in the spreadsheet getting the cell values
            for (int i = 1; i <= socSkillsMatrixWorksheet.LastRowNum; ++i)
            {
                // get the row data
                var row = socSkillsMatrixWorksheet.GetRow(i);

                // get the individual cell data
                var socSkillsMatrixWorksheetRow = GetSocSkillsMatrixWorksheetRowData(socSkillsMatrixWorksheetColumnIndexModel, row);

                // get the cell data for the dictionary key
                string url = socSkillsMatrixWorksheetRow.ItemDefaultUrl.TrimStart('/');

                // add the apprenticeshipRequirementSpreadsheet to the dictionary
                socSkillsMatrixWorksheetDictionary.Add(url, socSkillsMatrixWorksheetRow);
            }

            IReadOnlyDictionary<string, SocSkillsMatrixWorksheetRowModel> readonlyWorksheet = socSkillsMatrixWorksheetDictionary;

            return readonlyWorksheet;
        }

        private void InitialiseSocSkillsMatrixWorksheetColumnIndexes(
            NPOI.SS.UserModel.ISheet socSkillsMatrixWorksheet,
            SocSkillsMatrixWorksheetColumnIndexModel socSkillsMatrixWorksheetColumnIndexModel,
            SocSkillsMatrixWorksheetRowModel socSkillsMatrixWorksheetRowModel)
        {
            // SystemParentIdColumnIndex
            socSkillsMatrixWorksheetColumnIndexModel.SystemParentIdColumnIndex = socSkillsMatrixWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(socSkillsMatrixWorksheetRowModel.SystemParentId))
                .ColumnIndex;

            // IncludeInSitemapColumnIndex
            socSkillsMatrixWorksheetColumnIndexModel.IncludeInSitemapColumnIndex = socSkillsMatrixWorksheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(socSkillsMatrixWorksheetRowModel.IncludeInSitemap))
                  .ColumnIndex;

            // IdColumnIndex
            socSkillsMatrixWorksheetColumnIndexModel.IdColumnIndex = socSkillsMatrixWorksheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(socSkillsMatrixWorksheetRowModel.Id))
                  .ColumnIndex;

            // DateCreatedColumnIndex
            socSkillsMatrixWorksheetColumnIndexModel.DateCreatedColumnIndex = socSkillsMatrixWorksheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(socSkillsMatrixWorksheetRowModel.DateCreated))
                  .ColumnIndex;

            // ItemDefaultUrlColumnIndex
            socSkillsMatrixWorksheetColumnIndexModel.ItemDefaultUrlColumnIndex = socSkillsMatrixWorksheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(socSkillsMatrixWorksheetRowModel.ItemDefaultUrl))
                  .ColumnIndex;

            // UrlNameColumnIndex
            socSkillsMatrixWorksheetColumnIndexModel.UrlNameColumnIndex = socSkillsMatrixWorksheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(socSkillsMatrixWorksheetRowModel.UrlName))
                  .ColumnIndex;

            // PublicationDateColumnIndex
            socSkillsMatrixWorksheetColumnIndexModel.PublicationDateColumnIndex = socSkillsMatrixWorksheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(socSkillsMatrixWorksheetRowModel.PublicationDate))
                  .ColumnIndex;

            // TitleColumnIndex
            socSkillsMatrixWorksheetColumnIndexModel.TitleColumnIndex = socSkillsMatrixWorksheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(socSkillsMatrixWorksheetRowModel.Title))
                  .ColumnIndex;

            // ONetAttributeTypeColumnIndex
            socSkillsMatrixWorksheetColumnIndexModel.ONetAttributeTypeColumnIndex = socSkillsMatrixWorksheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(socSkillsMatrixWorksheetRowModel.ONetAttributeType))
                  .ColumnIndex;

            // RankColumnIndex
            socSkillsMatrixWorksheetColumnIndexModel.RankColumnIndex = socSkillsMatrixWorksheet
                  .GetRow(0)
                  .Cells
                  .Single(x => x.StringCellValue == nameof(socSkillsMatrixWorksheetRowModel.Rank))
                  .ColumnIndex;
        }

        private SocSkillsMatrixWorksheetRowModel GetSocSkillsMatrixWorksheetRowData(
            SocSkillsMatrixWorksheetColumnIndexModel socSkillsMatrixWorksheetColumnIndexModel,
            NPOI.SS.UserModel.IRow socSkillsMatrixWorksheetRow)
        {
            var socSkillsMatrixWorksheetRowModel = new SocSkillsMatrixWorksheetRowModel();

            // SystemParentId
            socSkillsMatrixWorksheetRowModel.SystemParentId = socSkillsMatrixWorksheetRow
                .GetCell(socSkillsMatrixWorksheetColumnIndexModel
                .SystemParentIdColumnIndex)
                .StringCellValue;

            // IncludeInSitemap
            socSkillsMatrixWorksheetRowModel.IncludeInSitemap = Convert.ToBoolean(socSkillsMatrixWorksheetRow
                .GetCell(socSkillsMatrixWorksheetColumnIndexModel
                .IncludeInSitemapColumnIndex)
                .StringCellValue);

            // Id
            socSkillsMatrixWorksheetRowModel.Id = socSkillsMatrixWorksheetRow
                .GetCell(socSkillsMatrixWorksheetColumnIndexModel
                .IdColumnIndex)
                .StringCellValue;

            // DateCreated
            socSkillsMatrixWorksheetRowModel.DateCreated = Convert.ToDateTime(socSkillsMatrixWorksheetRow
                .GetCell(socSkillsMatrixWorksheetColumnIndexModel
                .DateCreatedColumnIndex)
                .StringCellValue);

            // ItemDefaultUrl
            socSkillsMatrixWorksheetRowModel.ItemDefaultUrl = socSkillsMatrixWorksheetRow
                .GetCell(socSkillsMatrixWorksheetColumnIndexModel
                .ItemDefaultUrlColumnIndex)
                .StringCellValue;

            // UrlName
            socSkillsMatrixWorksheetRowModel.UrlName = socSkillsMatrixWorksheetRow
                .GetCell(socSkillsMatrixWorksheetColumnIndexModel
                .UrlNameColumnIndex)
                .StringCellValue;

            // PublicationDate
            socSkillsMatrixWorksheetRowModel.PublicationDate = Convert.ToDateTime(socSkillsMatrixWorksheetRow
                .GetCell(socSkillsMatrixWorksheetColumnIndexModel
                .PublicationDateColumnIndex)
                .StringCellValue);

            // Title
            socSkillsMatrixWorksheetRowModel.Title = socSkillsMatrixWorksheetRow
                .GetCell(socSkillsMatrixWorksheetColumnIndexModel
                .TitleColumnIndex)
                .StringCellValue;

            // ONetAttributeType
            socSkillsMatrixWorksheetRowModel.ONetAttributeType = socSkillsMatrixWorksheetRow
                .GetCell(socSkillsMatrixWorksheetColumnIndexModel
                .ONetAttributeTypeColumnIndex)
                .StringCellValue;

            // Rank
            socSkillsMatrixWorksheetRowModel.Rank = Convert.ToByte(socSkillsMatrixWorksheetRow
                .GetCell(socSkillsMatrixWorksheetColumnIndexModel
                .RankColumnIndex)
                .StringCellValue);

            return socSkillsMatrixWorksheetRowModel;
        }
        #endregion SocSkillsMatrix Worksheet

        #region ApprenticeshipRequirement Worksheet

        private IReadOnlyDictionary<string, ApprenticeshipRequirementWorksheetRowModel> ImportApprenticeshipRequirementWorksheet(XSSFWorkbook jobProfilesWorkbook)
        {
            // Initialise the dictionary that will hold the ApprenticeshipRequirement worksheet row/columns model
            var apprenticeshipRequirementWorksheetDictionary = new Dictionary<string, ApprenticeshipRequirementWorksheetRowModel>();

            // Get a reference to the ApprenticeshipRequirement worksheet in the job profile workbook
            var apprenticeshipRequirementWorksheet = jobProfilesWorkbook.GetSheet("ApprenticeshipRequirement");

            // Initialise the model to hold the columns indexes
            var apprenticeshipRequirementWorksheetColumnIndexModel = new ApprenticeshipRequirementWorksheetColumnIndexModel();

            // Initialise a row model so that we can use the column names to get the column indexes
            var apprenticeshipRequirementWorksheetRowModel = new ApprenticeshipRequirementWorksheetRowModel();

            // Inialise the column index values
            InitialiseApprenticeshipRequirementWorksheetColumnIndexes(apprenticeshipRequirementWorksheet, apprenticeshipRequirementWorksheetColumnIndexModel, apprenticeshipRequirementWorksheetRowModel);

            // iterate the rows in the spreadsheet getting the cell values
            for (int i = 1; i <= apprenticeshipRequirementWorksheet.LastRowNum; ++i)
            {
                // get the row data
                var row = apprenticeshipRequirementWorksheet.GetRow(i);

                // get the individual cell data
                var apprenticeshipRequirementWorksheetRow = GetApprenticeshipRequirementWorksheetRowData(apprenticeshipRequirementWorksheetColumnIndexModel, row);

                // get the cell data for the dictionary key
                string url = apprenticeshipRequirementWorksheetRow.ItemDefaultUrl.TrimStart('/');

                // add the apprenticeshipRequirementSpreadsheet to the dictionary
                apprenticeshipRequirementWorksheetDictionary.Add(url, apprenticeshipRequirementWorksheetRow);
            }

            IReadOnlyDictionary<string, ApprenticeshipRequirementWorksheetRowModel> readonlyWorksheet = apprenticeshipRequirementWorksheetDictionary;

            return readonlyWorksheet;
        }

        private void InitialiseApprenticeshipRequirementWorksheetColumnIndexes(
            NPOI.SS.UserModel.ISheet apprenticeshipRequirementWorksheet,
            ApprenticeshipRequirementWorksheetColumnIndexModel apprenticeshipRequirementWorksheetColumnIndexModel,
            ApprenticeshipRequirementWorksheetRowModel apprenticeshipRequirementWorksheetRowModel)
        {
            // SystemParentIdColumnIndex
            apprenticeshipRequirementWorksheetColumnIndexModel.SystemParentIdColumnIndex = apprenticeshipRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(apprenticeshipRequirementWorksheetRowModel.SystemParentId))
                .ColumnIndex;

            // IncludeInSitemapColumnIndex
            apprenticeshipRequirementWorksheetColumnIndexModel.IncludeInSitemapColumnIndex = apprenticeshipRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(apprenticeshipRequirementWorksheetRowModel.IncludeInSitemap))
                .ColumnIndex;

            // IdColumnIndex
            apprenticeshipRequirementWorksheetColumnIndexModel.IdColumnIndex = apprenticeshipRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(apprenticeshipRequirementWorksheetRowModel.Id))
                .ColumnIndex;

            // DateCreatedColumnIndex
            apprenticeshipRequirementWorksheetColumnIndexModel.DateCreatedColumnIndex = apprenticeshipRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(apprenticeshipRequirementWorksheetRowModel.DateCreated))
                .ColumnIndex;

            // ItemDefaultUrlColumnIndex
            apprenticeshipRequirementWorksheetColumnIndexModel.ItemDefaultUrlColumnIndex = apprenticeshipRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(apprenticeshipRequirementWorksheetRowModel.ItemDefaultUrl))
                .ColumnIndex;

            // UrlNameColumnIndex
            apprenticeshipRequirementWorksheetColumnIndexModel.UrlNameColumnIndex = apprenticeshipRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(apprenticeshipRequirementWorksheetRowModel.UrlName))
                .ColumnIndex;

            // PublicationDateColumnIndex
            apprenticeshipRequirementWorksheetColumnIndexModel.PublicationDateColumnIndex = apprenticeshipRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(apprenticeshipRequirementWorksheetRowModel.PublicationDate))
                .ColumnIndex;

            // TitleColumnIndex
            apprenticeshipRequirementWorksheetColumnIndexModel.TitleColumnIndex = apprenticeshipRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(apprenticeshipRequirementWorksheetRowModel.Title))
                .ColumnIndex;

            // InfoColumnIndex
            apprenticeshipRequirementWorksheetColumnIndexModel.InfoColumnIndex = apprenticeshipRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(apprenticeshipRequirementWorksheetRowModel.Info))
                .ColumnIndex;
        }

        private ApprenticeshipRequirementWorksheetRowModel GetApprenticeshipRequirementWorksheetRowData(
            ApprenticeshipRequirementWorksheetColumnIndexModel apprenticeshipRequirementWorksheetColumnIndexModel,
            NPOI.SS.UserModel.IRow apprenticeshipRequirementWorksheetRow)
        {
            var apprenticeshipRequirementWorksheetRowModel = new ApprenticeshipRequirementWorksheetRowModel();

            // SystemParentId
            apprenticeshipRequirementWorksheetRowModel.SystemParentId = apprenticeshipRequirementWorksheetRow
                .GetCell(apprenticeshipRequirementWorksheetColumnIndexModel
                .SystemParentIdColumnIndex)
                .StringCellValue;

            // IncludeInSitemap
            apprenticeshipRequirementWorksheetRowModel.IncludeInSitemap = Convert.ToBoolean(apprenticeshipRequirementWorksheetRow
                .GetCell(apprenticeshipRequirementWorksheetColumnIndexModel
                .IncludeInSitemapColumnIndex)
                .StringCellValue);

            // Id
            apprenticeshipRequirementWorksheetRowModel.Id = apprenticeshipRequirementWorksheetRow
                .GetCell(apprenticeshipRequirementWorksheetColumnIndexModel
                .IdColumnIndex)
                .StringCellValue;

            // DateCreated
            apprenticeshipRequirementWorksheetRowModel.DateCreated = Convert.ToDateTime(apprenticeshipRequirementWorksheetRow
                .GetCell(apprenticeshipRequirementWorksheetColumnIndexModel
                .DateCreatedColumnIndex)
                .StringCellValue);

            // ItemDefaultUrl
            apprenticeshipRequirementWorksheetRowModel.ItemDefaultUrl = apprenticeshipRequirementWorksheetRow
                .GetCell(apprenticeshipRequirementWorksheetColumnIndexModel
                .ItemDefaultUrlColumnIndex)
                .StringCellValue;

            // UrlName
            apprenticeshipRequirementWorksheetRowModel.UrlName = apprenticeshipRequirementWorksheetRow
                .GetCell(apprenticeshipRequirementWorksheetColumnIndexModel
                .UrlNameColumnIndex)
                .StringCellValue;

            // PublicationDate
            apprenticeshipRequirementWorksheetRowModel.PublicationDate = Convert.ToDateTime(apprenticeshipRequirementWorksheetRow
                .GetCell(apprenticeshipRequirementWorksheetColumnIndexModel
                .PublicationDateColumnIndex)
                .StringCellValue);

            // Title
            apprenticeshipRequirementWorksheetRowModel.Title = apprenticeshipRequirementWorksheetRow
                .GetCell(apprenticeshipRequirementWorksheetColumnIndexModel
                .TitleColumnIndex)
                .StringCellValue;

            // Info
            apprenticeshipRequirementWorksheetRowModel.Info = apprenticeshipRequirementWorksheetRow
                .GetCell(apprenticeshipRequirementWorksheetColumnIndexModel
                .InfoColumnIndex)
                .StringCellValue;

            return apprenticeshipRequirementWorksheetRowModel;
        }

        #endregion ApprenticeshipRequirement Worksheet

        #region CollegeRequirement worksheet

        private IReadOnlyDictionary<string, CollegeRequirementWorksheetRowModel> ImportCollegeRequirementWorksheet(XSSFWorkbook jobProfilesWorkbook)
        {
            // Initialise the dictionary that will hold the CollegeRequirement worksheet row/columns model
            var collegeRequirementWorksheetDictionary = new Dictionary<string, CollegeRequirementWorksheetRowModel>();

            // Get a reference to the CollegeRequirement worksheet in the job profile workbook
            var collegeRequirementWorksheet = jobProfilesWorkbook.GetSheet("CollegeRequirement");

            // Initialise the model to hold the columns indexes
            var collegeRequirementWorksheetColumnIndexModel = new CollegeRequirementWorksheetColumnIndexModel();

            // Initialise a row model so that we can use the column names to get the column indexes
            var collegeRequirementWorksheetRowModel = new CollegeRequirementWorksheetRowModel();

            // Inialise the column index values
            InitialiseCollegeRequirementWorksheetColumnIndexes(collegeRequirementWorksheet, collegeRequirementWorksheetColumnIndexModel, collegeRequirementWorksheetRowModel);

            // iterate the rows in the spreadsheet getting the cell values
            for (int i = 1; i <= collegeRequirementWorksheet.LastRowNum; ++i)
            {
                // get the row data
                var row = collegeRequirementWorksheet.GetRow(i);

                // get the individual cell data
                var collegeRequirementWorksheetRow = GetCollegeRequirementWorksheetRowData(collegeRequirementWorksheetColumnIndexModel, row);

                // get the cell data for the key
                string url = collegeRequirementWorksheetRow.ItemDefaultUrl.TrimStart('/');

                // add the collegeRequirementSpreadsheet to the dictionary
                collegeRequirementWorksheetDictionary.Add(url, collegeRequirementWorksheetRow);
            }

            IReadOnlyDictionary<string, CollegeRequirementWorksheetRowModel> readonlyWorksheet = collegeRequirementWorksheetDictionary;

            return readonlyWorksheet;
        }

        private void InitialiseCollegeRequirementWorksheetColumnIndexes(
            NPOI.SS.UserModel.ISheet collegeRequirementWorksheet,
            CollegeRequirementWorksheetColumnIndexModel collegeRequirementWorksheetColumnIndexModel,
            CollegeRequirementWorksheetRowModel collegeRequirementWorksheetRowModel)
        {
            // SystemParentIdColumnIndex
            collegeRequirementWorksheetColumnIndexModel.SystemParentIdColumnIndex = collegeRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(collegeRequirementWorksheetRowModel.SystemParentId))
                .ColumnIndex;

            // IncludeInSitemapColumnIndex
            collegeRequirementWorksheetColumnIndexModel.IncludeInSitemapColumnIndex = collegeRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(collegeRequirementWorksheetRowModel.IncludeInSitemap))
                .ColumnIndex;

            // IdColumnIndex
            collegeRequirementWorksheetColumnIndexModel.IdColumnIndex = collegeRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(collegeRequirementWorksheetRowModel.Id))
                .ColumnIndex;

            // DateCreatedColumnIndex
            collegeRequirementWorksheetColumnIndexModel.DateCreatedColumnIndex = collegeRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(collegeRequirementWorksheetRowModel.DateCreated))
                .ColumnIndex;

            // ItemDefaultUrlColumnIndex
            collegeRequirementWorksheetColumnIndexModel.ItemDefaultUrlColumnIndex = collegeRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(collegeRequirementWorksheetRowModel.ItemDefaultUrl))
                .ColumnIndex;

            // UrlNameColumnIndex
            collegeRequirementWorksheetColumnIndexModel.UrlNameColumnIndex = collegeRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(collegeRequirementWorksheetRowModel.UrlName))
                .ColumnIndex;

            // PublicationDateColumnIndex
            collegeRequirementWorksheetColumnIndexModel.PublicationDateColumnIndex = collegeRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(collegeRequirementWorksheetRowModel.PublicationDate))
                .ColumnIndex;

            // TitleColumnIndex
            collegeRequirementWorksheetColumnIndexModel.TitleColumnIndex = collegeRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(collegeRequirementWorksheetRowModel.Title))
                .ColumnIndex;

            // InfoColumnIndex
            collegeRequirementWorksheetColumnIndexModel.InfoColumnIndex = collegeRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(collegeRequirementWorksheetRowModel.Info))
                .ColumnIndex;
        }

        private CollegeRequirementWorksheetRowModel GetCollegeRequirementWorksheetRowData(
            CollegeRequirementWorksheetColumnIndexModel collegeRequirementWorksheetColumnIndexModel,
            NPOI.SS.UserModel.IRow collegeRequirementWorksheetRow)
        {
            var collegeRequirementWorksheetRowModel = new CollegeRequirementWorksheetRowModel();

            // SystemParentId
            collegeRequirementWorksheetRowModel.SystemParentId = collegeRequirementWorksheetRow
                .GetCell(collegeRequirementWorksheetColumnIndexModel
                .SystemParentIdColumnIndex)
                .StringCellValue;

            // IncludeInSitemap
            collegeRequirementWorksheetRowModel.IncludeInSitemap = Convert.ToBoolean(collegeRequirementWorksheetRow
                .GetCell(collegeRequirementWorksheetColumnIndexModel
                .IncludeInSitemapColumnIndex)
                .StringCellValue);

            // Id
            collegeRequirementWorksheetRowModel.Id = collegeRequirementWorksheetRow
                .GetCell(collegeRequirementWorksheetColumnIndexModel
                .IdColumnIndex)
                .StringCellValue;

            // DateCreated
            collegeRequirementWorksheetRowModel.DateCreated = Convert.ToDateTime(collegeRequirementWorksheetRow
                .GetCell(collegeRequirementWorksheetColumnIndexModel
                .DateCreatedColumnIndex)
                .StringCellValue);

            // ItemDefaultUrl
            collegeRequirementWorksheetRowModel.ItemDefaultUrl = collegeRequirementWorksheetRow
                .GetCell(collegeRequirementWorksheetColumnIndexModel
                .ItemDefaultUrlColumnIndex)
                .StringCellValue;

            // UrlName
            collegeRequirementWorksheetRowModel.UrlName = collegeRequirementWorksheetRow
                .GetCell(collegeRequirementWorksheetColumnIndexModel
                .UrlNameColumnIndex)
                .StringCellValue;

            // PublicationDate
            collegeRequirementWorksheetRowModel.PublicationDate = Convert.ToDateTime(collegeRequirementWorksheetRow
                .GetCell(collegeRequirementWorksheetColumnIndexModel
                .PublicationDateColumnIndex)
                .StringCellValue);

            // Title
            collegeRequirementWorksheetRowModel.Title = collegeRequirementWorksheetRow
                .GetCell(collegeRequirementWorksheetColumnIndexModel
                .TitleColumnIndex)
                .StringCellValue;

            // Info
            collegeRequirementWorksheetRowModel.Info = collegeRequirementWorksheetRow
                .GetCell(collegeRequirementWorksheetColumnIndexModel
                .InfoColumnIndex)
                .StringCellValue;

            return collegeRequirementWorksheetRowModel;
        }

        #endregion CollegeRequirement Worksheet

        #region UniversityRequirement Worksheet

        private IReadOnlyDictionary<string, UniversityRequirementWorksheetRowModel> ImportUniversityRequirementWorksheet(XSSFWorkbook jobProfilesWorkbook)
        {
            // Initialise the dictionary that will hold the UniversityRequirement worksheet row/columns model
            var universityRequirementWorksheetDictionary = new Dictionary<string, UniversityRequirementWorksheetRowModel>();

            // Get a reference to the UniversityRequirement worksheet in the job profile workbook
            var universityRequirementWorksheet = jobProfilesWorkbook.GetSheet("UniversityRequirement");

            // Initialise the model to hold the columns indexes
            var universityRequirementWorksheetColumnIndexModel = new UniversityRequirementWorksheetColumnIndexModel();

            // Initialise a row model so that we can use the column names to get the column indexes
            var universityRequirementWorksheetRowModel = new UniversityRequirementWorksheetRowModel();

            // Inialise the column index values
            InitialiseUniversityRequirementWorksheetColumnIndexes(universityRequirementWorksheet, universityRequirementWorksheetColumnIndexModel, universityRequirementWorksheetRowModel);

            // iterate the rows in the spreadsheet getting the cell values
            for (int i = 1; i <= universityRequirementWorksheet.LastRowNum; ++i)
            {
                // get the row data
                var row = universityRequirementWorksheet.GetRow(i);

                // get the individual cell data
                var universityRequirementWorksheetRow = GetUniversityRequirementWorksheetRowData(universityRequirementWorksheetColumnIndexModel, row);

                // get the cell data for the key
                string url = universityRequirementWorksheetRow.ItemDefaultUrl.TrimStart('/');

                // add the universityRequirementSpreadsheet to the dictionary
                universityRequirementWorksheetDictionary.Add(url, universityRequirementWorksheetRow);
            }

            IReadOnlyDictionary<string, UniversityRequirementWorksheetRowModel> readonlyWorksheet = universityRequirementWorksheetDictionary;

            return readonlyWorksheet;
        }

        private void InitialiseUniversityRequirementWorksheetColumnIndexes(
            NPOI.SS.UserModel.ISheet universityRequirementWorksheet,
            UniversityRequirementWorksheetColumnIndexModel universityRequirementWorksheetColumnIndexModel,
            UniversityRequirementWorksheetRowModel universityRequirementWorksheetRowModel)
        {
            // SystemParentIdColumnIndex
            universityRequirementWorksheetColumnIndexModel.SystemParentIdColumnIndex = universityRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(universityRequirementWorksheetRowModel.SystemParentId))
                .ColumnIndex;

            // IncludeInSitemapColumnIndex
            universityRequirementWorksheetColumnIndexModel.IncludeInSitemapColumnIndex = universityRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(universityRequirementWorksheetRowModel.IncludeInSitemap))
                .ColumnIndex;

            // IdColumnIndex
            universityRequirementWorksheetColumnIndexModel.IdColumnIndex = universityRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(universityRequirementWorksheetRowModel.Id))
                .ColumnIndex;

            // DateCreatedColumnIndex
            universityRequirementWorksheetColumnIndexModel.DateCreatedColumnIndex = universityRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(universityRequirementWorksheetRowModel.DateCreated))
                .ColumnIndex;

            // ItemDefaultUrlColumnIndex
            universityRequirementWorksheetColumnIndexModel.ItemDefaultUrlColumnIndex = universityRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(universityRequirementWorksheetRowModel.ItemDefaultUrl))
                .ColumnIndex;

            // UrlNameColumnIndex
            universityRequirementWorksheetColumnIndexModel.UrlNameColumnIndex = universityRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(universityRequirementWorksheetRowModel.UrlName))
                .ColumnIndex;

            // PublicationDateColumnIndex
            universityRequirementWorksheetColumnIndexModel.PublicationDateColumnIndex = universityRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(universityRequirementWorksheetRowModel.PublicationDate))
                .ColumnIndex;

            // TitleColumnIndex
            universityRequirementWorksheetColumnIndexModel.TitleColumnIndex = universityRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(universityRequirementWorksheetRowModel.Title))
                .ColumnIndex;

            // InfoColumnIndex
            universityRequirementWorksheetColumnIndexModel.InfoColumnIndex = universityRequirementWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(universityRequirementWorksheetRowModel.Info))
                .ColumnIndex;
        }

        private UniversityRequirementWorksheetRowModel GetUniversityRequirementWorksheetRowData(
            UniversityRequirementWorksheetColumnIndexModel universityRequirementWorksheetColumnIndexModel,
            NPOI.SS.UserModel.IRow universityRequirementWorksheetRow)
        {
            var universityRequirementWorksheetRowModel = new UniversityRequirementWorksheetRowModel();

            // SystemParentId
            universityRequirementWorksheetRowModel.SystemParentId = universityRequirementWorksheetRow
                .GetCell(universityRequirementWorksheetColumnIndexModel
                .SystemParentIdColumnIndex)
                .StringCellValue;

            // IncludeInSitemap
            universityRequirementWorksheetRowModel.IncludeInSitemap = Convert.ToBoolean(universityRequirementWorksheetRow
                .GetCell(universityRequirementWorksheetColumnIndexModel
                .IncludeInSitemapColumnIndex)
                .StringCellValue);

            // Id
            universityRequirementWorksheetRowModel.Id = universityRequirementWorksheetRow
                .GetCell(universityRequirementWorksheetColumnIndexModel
                .IdColumnIndex)
                .StringCellValue;

            // DateCreated
            universityRequirementWorksheetRowModel.DateCreated = Convert.ToDateTime(universityRequirementWorksheetRow
                .GetCell(universityRequirementWorksheetColumnIndexModel
                .DateCreatedColumnIndex)
                .StringCellValue);

            // ItemDefaultUrl
            universityRequirementWorksheetRowModel.ItemDefaultUrl = universityRequirementWorksheetRow
                .GetCell(universityRequirementWorksheetColumnIndexModel
                .ItemDefaultUrlColumnIndex)
                .StringCellValue;

            // UrlName
            universityRequirementWorksheetRowModel.UrlName = universityRequirementWorksheetRow
                .GetCell(universityRequirementWorksheetColumnIndexModel
                .UrlNameColumnIndex)
                .StringCellValue;

            // PublicationDate
            universityRequirementWorksheetRowModel.PublicationDate = Convert.ToDateTime(universityRequirementWorksheetRow
                .GetCell(universityRequirementWorksheetColumnIndexModel
                .PublicationDateColumnIndex)
                .StringCellValue);

            // Title
            universityRequirementWorksheetRowModel.Title = universityRequirementWorksheetRow
                .GetCell(universityRequirementWorksheetColumnIndexModel
                .TitleColumnIndex)
                .StringCellValue;

            // Info
            universityRequirementWorksheetRowModel.Info = universityRequirementWorksheetRow
                .GetCell(universityRequirementWorksheetColumnIndexModel
                .InfoColumnIndex)
                .StringCellValue;

            return universityRequirementWorksheetRowModel;
        }

        #endregion UniversityRequirement Worksheet

        #region JobProfile Worksheet

        private IReadOnlyDictionary<string, JobProfileWorksheetRowModel> ImportJobProfileWorksheet(XSSFWorkbook jobProfilesWorkbook)
        {
            // Initialise the dictionary that will hold the job profile worksheet row/columns model
            var jobProfileWorksheetKeyValueDictionary = new Dictionary<string, JobProfileWorksheetRowModel>();

            // Get a reference to the job profile worksheet in the job profile workbook
            var jobProfileWorksheet = jobProfilesWorkbook.GetSheet("JobProfile");

            // Initialise the model to hold the columns from the job profile worksheet
            //var jobProfileWorksheetColumnModel = new JobProfileWorksheetColumnModel();

            // Initialise the model the dole the columns index
            var jobProfileWorksheetColumnIndexModel = new JobProfileWorksheetColumnIndexModel();

            // Initialise a row model so that we can use the column names to get the column indexes
            var jobProfileWorksheetRowModel = new JobProfileWorksheetRowModel();

            // Inialise the column index values
            InitialiseJobProfileWorksheetColumnIndexes(jobProfileWorksheet, jobProfileWorksheetColumnIndexModel, jobProfileWorksheetRowModel);

            // iterate the rows in the spreadsheet getting the cell values
            for (int i = 1; i <= jobProfileWorksheet.LastRowNum; ++i)
            {
                // get the row data
                var row = jobProfileWorksheet.GetRow(i);

                // get the individual cell data
                var jobProfileWorksheetRow = GetJobProfileWorksheetRowData(jobProfileWorksheetColumnIndexModel, row);

                // get the cell data for the key
                string url = jobProfileWorksheetRow.ItemDefaultUrl.TrimStart('/');

                // add the jobProfileSpreadsheet to the dictionary
                jobProfileWorksheetKeyValueDictionary.Add(url, jobProfileWorksheetRow);
            }

            IReadOnlyDictionary<string, JobProfileWorksheetRowModel> worksheet = jobProfileWorksheetKeyValueDictionary;

            return worksheet;
        }

        private void InitialiseJobProfileWorksheetColumnIndexes(
            NPOI.SS.UserModel.ISheet jobProfileWorksheet,
            JobProfileWorksheetColumnIndexModel jobProfileWorksheetColumnIndexModel,
            JobProfileWorksheetRowModel jobProfileWorksheetRowModel)
        {
            // SystemParentIdColumnIndex
            jobProfileWorksheetColumnIndexModel.SystemParentIdColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.SystemParentId))
                .ColumnIndex;

            // IncludeInSitemapColumnIndex
            jobProfileWorksheetColumnIndexModel.IncludeInSitemapColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.IncludeInSitemap))
                .ColumnIndex;

            // IdColumnIndex
            jobProfileWorksheetColumnIndexModel.IdColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.Id))
                .ColumnIndex;

            // DateCreatedColumnIndex
            jobProfileWorksheetColumnIndexModel.DateCreatedColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.DateCreated))
                .ColumnIndex;

            // ItemDefaultUrlColumnIndex
            jobProfileWorksheetColumnIndexModel.ItemDefaultUrlColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.ItemDefaultUrl))
                .ColumnIndex;

            // UrlNameColumnIndex
            jobProfileWorksheetColumnIndexModel.UrlNameColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.UrlName))
                .ColumnIndex;

            // PublicationDateColumnIndex
            jobProfileWorksheetColumnIndexModel.PublicationDateColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.PublicationDate))
                .ColumnIndex;

            // IsLMISalaryFeedOverridenColumnIndex
            jobProfileWorksheetColumnIndexModel.IsLMISalaryFeedOverridenColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.IsLMISalaryFeedOverriden))
                .ColumnIndex;

            // MinimumHoursColumnIndex
            jobProfileWorksheetColumnIndexModel.MinimumHoursColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.MinimumHours))
                .ColumnIndex;

            // WhatYouWillDoColumnIndex
            jobProfileWorksheetColumnIndexModel.WhatYouWillDoColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.WhatYouWillDo))
                .ColumnIndex;

            // HiddenAlternativeTitleColumnIndex
            jobProfileWorksheetColumnIndexModel.HiddenAlternativeTitleColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.HiddenAlternativeTitle))
                .ColumnIndex;

            // IsImportedColumnIndex
            jobProfileWorksheetColumnIndexModel.IsImportedColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.IsImported))
                .ColumnIndex;

            // DynamicTitlePrefixColumnIndex
            jobProfileWorksheetColumnIndexModel.DynamicTitlePrefixColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.DynamicTitlePrefix))
                .ColumnIndex;

            // WYDIntroductionColumnIndex
            jobProfileWorksheetColumnIndexModel.WYDIntroductionColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.WYDIntroduction))
                .ColumnIndex;

            // WorkingHoursDetailsColumnIndex
            jobProfileWorksheetColumnIndexModel.WorkingHoursDetailsColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.WorkingHoursDetails))
                .ColumnIndex;

            // CareerTipsColumnIndex
            jobProfileWorksheetColumnIndexModel.CareerTipsColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.CareerTips))
                .ColumnIndex;

            // SalaryColumnIndex
            jobProfileWorksheetColumnIndexModel.SalaryColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.Salary))
                .ColumnIndex;

            // ApprenticeshipEntryRequirementsColumnIndex
            jobProfileWorksheetColumnIndexModel.ApprenticeshipEntryRequirementsColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.ApprenticeshipEntryRequirements))
                .ColumnIndex;

            // VolunteeringColumnIndex
            jobProfileWorksheetColumnIndexModel.VolunteeringColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.Volunteering))
                .ColumnIndex;

            // CollegeFurtherRouteInfoColumnIndex
            jobProfileWorksheetColumnIndexModel.CollegeFurtherRouteInfoColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.CollegeFurtherRouteInfo))
                .ColumnIndex;

            // SalaryRangeColumnIndex
            jobProfileWorksheetColumnIndexModel.SalaryRangeColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.SalaryRange))
                .ColumnIndex;

            // WorkingPatternDetailsColumnIndex
            jobProfileWorksheetColumnIndexModel.WorkingPatternDetailsColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.WorkingPatternDetails))
                .ColumnIndex;

            // OverviewColumnIndex
            jobProfileWorksheetColumnIndexModel.OverviewColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.Overview))
                .ColumnIndex;

            // BAUSystemOverrideUrlColumnIndex
            jobProfileWorksheetColumnIndexModel.BAUSystemOverrideUrlColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.BAUSystemOverrideUrl))
                .ColumnIndex;

            // CollegeRelevantSubjectsColumnIndex
            jobProfileWorksheetColumnIndexModel.CollegeRelevantSubjectsColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.CollegeRelevantSubjects))
                .ColumnIndex;

            // JobProfileCategoriesColumnIndex
            jobProfileWorksheetColumnIndexModel.JobProfileCategoriesColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.JobProfileCategories))
                .ColumnIndex;

            // IsWYDCadReadyColumnIndex
            jobProfileWorksheetColumnIndexModel.IsWYDCadReadyColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.IsWYDCadReady))
                .ColumnIndex;

            // ProfessionalAndIndustryBodiesColumnIndex
            jobProfileWorksheetColumnIndexModel.ProfessionalAndIndustryBodiesColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.ProfessionalAndIndustryBodies))
                .ColumnIndex;

            // UniversityFurtherRouteInfoColumnIndex
            jobProfileWorksheetColumnIndexModel.UniversityFurtherRouteInfoColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.UniversityFurtherRouteInfo))
                .ColumnIndex;

            // ApprenticeshipFurtherRouteInfoColumnIndex
            jobProfileWorksheetColumnIndexModel.ApprenticeshipFurtherRouteInfoColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.ApprenticeshipFurtherRouteInfo))
                .ColumnIndex;

            // OtherRoutesColumnIndex
            jobProfileWorksheetColumnIndexModel.OtherRoutesColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.OtherRoutes))
                .ColumnIndex;

            // CourseKeywordsColumnIndex
            jobProfileWorksheetColumnIndexModel.CourseKeywordsColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.CourseKeywords))
                .ColumnIndex;

            // ApprenticeshipRelevantSubjectsColumnIndex
            jobProfileWorksheetColumnIndexModel.ApprenticeshipRelevantSubjectsColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.ApprenticeshipRelevantSubjects))
                .ColumnIndex;

            // CollegeEntryRequirementsColumnIndex
            jobProfileWorksheetColumnIndexModel.CollegeEntryRequirementsColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.CollegeEntryRequirements))
                .ColumnIndex;

            // UniversityRelevantSubjectsColumnIndex
            jobProfileWorksheetColumnIndexModel.UniversityRelevantSubjectsColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.UniversityRelevantSubjects))
                .ColumnIndex;

            // WorkingPatternColumnIndex
            jobProfileWorksheetColumnIndexModel.WorkingPatternColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.WorkingPattern))
                .ColumnIndex;

            // EntryRoutesColumnIndex
            jobProfileWorksheetColumnIndexModel.EntryRoutesColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.EntryRoutes))
                .ColumnIndex;

            // CareerPathProgressionColumnIndex
            jobProfileWorksheetColumnIndexModel.CareerPathAndProgressionColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.CareerPathAndProgression))
                .ColumnIndex;

            // DoesNotExistInBAUColumnIndex
            jobProfileWorksheetColumnIndexModel.DoesNotExistInBAUColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.DoesNotExistInBAU))
                .ColumnIndex;

            // OtherRequirementsColumnIndex
            jobProfileWorksheetColumnIndexModel.OtherRequirementsColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.OtherRequirements))
                .ColumnIndex;

            // WYDDayToDayTasksColumnIndex
            jobProfileWorksheetColumnIndexModel.WYDDayToDayTasksColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.WYDDayToDayTasks))
                .ColumnIndex;

            // DigitalSkillsLevelColumnIndex
            jobProfileWorksheetColumnIndexModel.DigitalSkillsLevelColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.DigitalSkillsLevel))
                .ColumnIndex;

            // JobProfileSpecialismColumnIndex
            jobProfileWorksheetColumnIndexModel.JobProfileSpecialismColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.JobProfileSpecialism))
                .ColumnIndex;

            // FurtherInformationColumnIndex
            jobProfileWorksheetColumnIndexModel.FurtherInformationColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.FurtherInformation))
                .ColumnIndex;

            // TitleColumnIndex
            jobProfileWorksheetColumnIndexModel.TitleColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.Title))
                .ColumnIndex;

            // HowToBecomeColumnIndex
            jobProfileWorksheetColumnIndexModel.HowToBecomeColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.HowToBecome))
                .ColumnIndex;

            // DirectApplicationColumnIndex
            jobProfileWorksheetColumnIndexModel.DirectApplicationColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.DirectApplication))
                .ColumnIndex;

            // MaximumHoursColumnIndex
            jobProfileWorksheetColumnIndexModel.MaximumHoursColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.MaximumHours))
                .ColumnIndex;

            // MaximumHoursColumnIndex
            jobProfileWorksheetColumnIndexModel.WorkingHoursPatternsAndEnvironmentColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.WorkingHoursPatternsAndEnvironment))
                .ColumnIndex;

            // SalaryExperiencedColumnIndex
            jobProfileWorksheetColumnIndexModel.SalaryExperiencedColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.SalaryExperienced))
                .ColumnIndex;

            // SkillsColumnIndex
            jobProfileWorksheetColumnIndexModel.SkillsColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.Skills))
                .ColumnIndex;

            // UniversityEntryRequirementsColumnIndex
            jobProfileWorksheetColumnIndexModel.UniversityEntryRequirementsColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.UniversityEntryRequirements))
                .ColumnIndex;

            // IsHTBCaDReadyColumnIndex
            jobProfileWorksheetColumnIndexModel.IsHTBCaDReadyColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.IsHTBCaDReady))
                .ColumnIndex;

            // WorkColumnIndex
            jobProfileWorksheetColumnIndexModel.WorkColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.Work))
                .ColumnIndex;

            // SalaryStarterColumnIndex
            jobProfileWorksheetColumnIndexModel.SalaryStarterColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.SalaryStarter))
                .ColumnIndex;

            // WidgetContentTitleColumnIndex
            jobProfileWorksheetColumnIndexModel.WidgetContentTitleColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.WidgetContentTitle))
                .ColumnIndex;

            // AlternativeTitleColumnIndex
            jobProfileWorksheetColumnIndexModel.AlternativeTitleColumnIndex = jobProfileWorksheet
                .GetRow(0)
                .Cells
                .Single(x => x.StringCellValue == nameof(jobProfileWorksheetRowModel.AlternativeTitle))
                .ColumnIndex;
        }

        private JobProfileWorksheetRowModel GetJobProfileWorksheetRowData(
            JobProfileWorksheetColumnIndexModel jobProfileWorksheetColumnIndexModel,
            NPOI.SS.UserModel.IRow jobProfileWorksheetRow)
        {
            var jobProfileWorksheetRowModel = new JobProfileWorksheetRowModel();

            // SystemParentId
            jobProfileWorksheetRowModel.SystemParentId = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .SystemParentIdColumnIndex)
                .StringCellValue;

            // IncludeInSitemap
            jobProfileWorksheetRowModel.IncludeInSitemap = Convert.ToBoolean(
                jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .IncludeInSitemapColumnIndex)
                .StringCellValue);

            // Id
            jobProfileWorksheetRowModel.Id = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .IdColumnIndex)
                .StringCellValue;

            // DateCreated
            jobProfileWorksheetRowModel.DateCreated = Convert.ToDateTime(
                jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .DateCreatedColumnIndex)
                .StringCellValue);

            // ItemDefaultUrl
            jobProfileWorksheetRowModel.ItemDefaultUrl = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .ItemDefaultUrlColumnIndex)
                .StringCellValue;

            // UrlName
            jobProfileWorksheetRowModel.UrlName = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .UrlNameColumnIndex)
                .StringCellValue;

            // PublicationDate
            jobProfileWorksheetRowModel.PublicationDate = Convert.ToDateTime(
                jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .PublicationDateColumnIndex)
                .StringCellValue);

            // IsLMISalaryFeedOverriden
            jobProfileWorksheetRowModel.IsLMISalaryFeedOverriden = Convert.ToBoolean(
                jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .IsLMISalaryFeedOverridenColumnIndex)
                .StringCellValue);

            // MinimumHours
            jobProfileWorksheetRowModel.MinimumHours = Convert.ToByte(
                Convert.ToDecimal(
                    jobProfileWorksheetRow
                    .GetCell(jobProfileWorksheetColumnIndexModel
                    .MinimumHoursColumnIndex)
                    .StringCellValue));

            // WhatYouWillDo
            jobProfileWorksheetRowModel.WhatYouWillDo = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .WhatYouWillDoColumnIndex)
                .StringCellValue;

            // HiddenAlternativeTitle
            jobProfileWorksheetRowModel.HiddenAlternativeTitle = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .HiddenAlternativeTitleColumnIndex)
                .StringCellValue;

            // IsImported
            jobProfileWorksheetRowModel.IsImported = Convert.ToBoolean(
                jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .IsImportedColumnIndex)
                .StringCellValue);

            // DynamicTitlePrefix
            jobProfileWorksheetRowModel.DynamicTitlePrefix = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .DynamicTitlePrefixColumnIndex)
                .StringCellValue;

            // WYDIntroduction
            jobProfileWorksheetRowModel.WYDIntroduction = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .WYDIntroductionColumnIndex)
                .StringCellValue;

            // WorkingHoursDetails
            jobProfileWorksheetRowModel.WorkingHoursDetails = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .WorkingHoursDetailsColumnIndex)
                .StringCellValue;

            // CareerTips
            jobProfileWorksheetRowModel.CareerTips = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .CareerTipsColumnIndex)
                .StringCellValue;

            // Salary
            jobProfileWorksheetRowModel.Salary = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .SalaryColumnIndex)
                .StringCellValue;

            // ApprenticeshipEntryRequirements
            jobProfileWorksheetRowModel.ApprenticeshipEntryRequirements = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .ApprenticeshipEntryRequirementsColumnIndex)
                .StringCellValue;

            // Volunteering
            jobProfileWorksheetRowModel.Volunteering = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .VolunteeringColumnIndex)
                .StringCellValue;

            // CollegeFurtherRouteInfo
            jobProfileWorksheetRowModel.CollegeFurtherRouteInfo = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .CollegeFurtherRouteInfoColumnIndex)
                .StringCellValue;

            // SalaryRange
            jobProfileWorksheetRowModel.SalaryRange = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .SalaryRangeColumnIndex)
                .StringCellValue;

            // WorkingPatternDetails
            jobProfileWorksheetRowModel.WorkingPatternDetails = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .WorkingPatternDetailsColumnIndex)
                .StringCellValue;

            // Overview
            jobProfileWorksheetRowModel.Overview = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .OverviewColumnIndex)
                .StringCellValue;

            // BAUSystemOverrideUrl
            jobProfileWorksheetRowModel.BAUSystemOverrideUrl = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .BAUSystemOverrideUrlColumnIndex)
                .StringCellValue;

            // CollegeRelevantSubjects
            jobProfileWorksheetRowModel.CollegeRelevantSubjects = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .CollegeRelevantSubjectsColumnIndex)
                .StringCellValue;

            // JobProfileCategories
            jobProfileWorksheetRowModel.JobProfileCategories = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .JobProfileCategoriesColumnIndex)
                .StringCellValue;

            // IsWYDCadReady
            jobProfileWorksheetRowModel.IsWYDCadReady = Convert.ToBoolean(
                jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .IsWYDCadReadyColumnIndex)
                .StringCellValue);

            // ProfessionalAndIndustryBodies
            jobProfileWorksheetRowModel.ProfessionalAndIndustryBodies = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .ProfessionalAndIndustryBodiesColumnIndex)
                .StringCellValue;

            // UniversityFurtherRouteInfo
            jobProfileWorksheetRowModel.UniversityFurtherRouteInfo = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .UniversityFurtherRouteInfoColumnIndex)
                .StringCellValue;

            // ApprenticeshipFurtherRouteInfo
            jobProfileWorksheetRowModel.ApprenticeshipFurtherRouteInfo = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .ApprenticeshipFurtherRouteInfoColumnIndex)
                .StringCellValue;

            // OtherRoutes
            jobProfileWorksheetRowModel.OtherRoutes = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .OtherRoutesColumnIndex)
                .StringCellValue;

            // CourseKeywords
            jobProfileWorksheetRowModel.CourseKeywords = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .CourseKeywordsColumnIndex)
                .StringCellValue;

            // ApprenticeshipRelevantSubjects
            jobProfileWorksheetRowModel.ApprenticeshipRelevantSubjects = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .ApprenticeshipRelevantSubjectsColumnIndex)
                .StringCellValue;

            // CollegeEntryRequirements
            jobProfileWorksheetRowModel.CollegeEntryRequirements = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .CollegeEntryRequirementsColumnIndex)
                .StringCellValue;

            // UniversityRelevantSubjects
            jobProfileWorksheetRowModel.UniversityRelevantSubjects = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .UniversityRelevantSubjectsColumnIndex)
                .StringCellValue;

            // WorkingPattern
            jobProfileWorksheetRowModel.WorkingPattern = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .WorkingPatternColumnIndex)
                .StringCellValue;

            // EntryRoutes
            jobProfileWorksheetRowModel.EntryRoutes = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .EntryRoutesColumnIndex)
                .StringCellValue;

            // CareerPathProgression
            jobProfileWorksheetRowModel.CareerPathAndProgression = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .CareerPathAndProgressionColumnIndex)
                .StringCellValue;

            // DoesNotExistInBAU
            jobProfileWorksheetRowModel.DoesNotExistInBAU = Convert.ToBoolean(
                jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .DoesNotExistInBAUColumnIndex)
                .StringCellValue);

            // OtherRequirements
            jobProfileWorksheetRowModel.OtherRequirements = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .OtherRequirementsColumnIndex)
                .StringCellValue;

            // WYDDayToDayTasks
            jobProfileWorksheetRowModel.WYDDayToDayTasks = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .WYDDayToDayTasksColumnIndex)
                .StringCellValue;

            // DigitalSkillsLevel
            jobProfileWorksheetRowModel.DigitalSkillsLevel = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .DigitalSkillsLevelColumnIndex)
                .StringCellValue;

            // JobProfileSpecialism
            jobProfileWorksheetRowModel.JobProfileSpecialism = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .JobProfileSpecialismColumnIndex)
                .StringCellValue;

            // FurtherInformation
            jobProfileWorksheetRowModel.FurtherInformation = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .FurtherInformationColumnIndex)
                .StringCellValue;

            // Title
            jobProfileWorksheetRowModel.Title = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .TitleColumnIndex)
                .StringCellValue;

            // HowToBecome
            jobProfileWorksheetRowModel.HowToBecome = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .HowToBecomeColumnIndex)
                .StringCellValue;

            // DirectApplication
            jobProfileWorksheetRowModel.DirectApplication = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .DirectApplicationColumnIndex)
                .StringCellValue;

            // DirectApplication
            jobProfileWorksheetRowModel.MaximumHours = Convert.ToByte(
                Convert.ToDecimal(
                    jobProfileWorksheetRow
                    .GetCell(jobProfileWorksheetColumnIndexModel
                    .MaximumHoursColumnIndex)
                    .StringCellValue));

            // WorkingHoursPatternsAndEnvironment
            jobProfileWorksheetRowModel.WorkingHoursPatternsAndEnvironment = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .WorkingHoursPatternsAndEnvironmentColumnIndex)
                .StringCellValue;

            // SalaryExperienced
            jobProfileWorksheetRowModel.SalaryExperienced = Convert.ToInt64(jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .SalaryExperiencedColumnIndex)
                .StringCellValue);

            // Skills
            jobProfileWorksheetRowModel.Skills = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .SkillsColumnIndex)
                .StringCellValue;

            // UniversityEntryRequirements
            jobProfileWorksheetRowModel.UniversityEntryRequirements = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .UniversityEntryRequirementsColumnIndex)
                .StringCellValue;

            // IsHTBCaDReady
            jobProfileWorksheetRowModel.IsHTBCaDReady = Convert.ToBoolean(
                jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .IsHTBCaDReadyColumnIndex)
                .StringCellValue);

            // Work
            jobProfileWorksheetRowModel.Work = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .WorkColumnIndex)
                .StringCellValue;

            // SalaryStarter
            jobProfileWorksheetRowModel.SalaryStarter = Convert.ToInt64(
                jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .SalaryStarterColumnIndex)
                .StringCellValue);

            // WidgetContentTitle
            jobProfileWorksheetRowModel.WidgetContentTitle = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .WidgetContentTitleColumnIndex)
                .StringCellValue;

            // WidgetContentTitle
            jobProfileWorksheetRowModel.AlternativeTitle = jobProfileWorksheetRow
                .GetCell(jobProfileWorksheetColumnIndexModel
                .AlternativeTitleColumnIndex)
                .StringCellValue;

            return jobProfileWorksheetRowModel;
        }

        #endregion JobProfile Worksheet
    }
}
