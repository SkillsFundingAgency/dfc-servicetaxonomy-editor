using System;
using System.Collections.Generic;
using System.IO;
using GetJobProfiles.Builders;
using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.Fields;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles.Models.Containers
{
    public class ReferenceData
    {
        // The job profile data spreadsheet exported from Sitefinity
        public XSSFWorkbook JobProfileExcelWorkbook { get; set; }

        // The dysac data spreadsheet exported from Sitefinity
        public XSSFWorkbook DysacExcelWorkbook { get; set; }

        // The dysac/job profile mapping data spreadsheet
        public XSSFWorkbook DysacJobProfileMappingExcelWorkbook { get; set; }

        // The SocSkillsMatrix spreadsheet
        public XSSFWorkbook SocSkillsMatricExcelWorkbook { get; set; }

        // Onet skill
        //public OnetSkillContentItemBuilder OnetSkillContentItemBuilder { get; set; }

        // University Requirement
        // University Link
        // College Requirement
        // College Link
        // Apprencticeship Requirement
        // Apprenticeship Link


        // SOC Codes Builder
        public SocCodeContentItemBuilder SocCodeContentItemBuilder { get; set; }

        // ONet Occupational Codes Builder
        public ONetOccupationalCodeContentItemBuilder ONetOccupationalCodeContentItemBuilder { get; set; }

        // ONet Skills Builder
        public OnetSkillContentItemBuilder OnetSkillContentItemBuilder { get; set;}

        // QCF Levels Builder
        public QcfLevelContentItemBuilder QcfLevelContentItemBuilder { get; set; }

        // JobProfiles Builder
        public JobProfileContentItemBuilder JobProfileContentItemBuilder { get; set; }

        // JobCategory Builder
        public JobCategoryBuilder JobCategoryBuilder { get; set; }

        // Onet Occupational Code Content Items
        public ONetOccupationalCodeContentItem ONetOccupationalCodeContentItems { get; set; }


        // Apprencticeship Standard Route
        public ApprenticeshipStandardRouteContentItem ApprenticeshipStandardRouteContentItems { get; set; }


        // University Route
        // College Route
        // Apprenticeship Route
        // Work Route
        // Volunteering Route
        // Direct Route
        // Other Route
        // Registration
        // Restriction
        // Other Requirement
        public OtherRequirementContentItem OtherRequirementContentItems { get; set; }

        // Digital Skills Level
        public DigitalSkillsLevelContentItem DigitalSkillsLevelContentItems { get; set; }

        // Working Environment
        // Working Location
        // Working Uniform
        // Apprencticeship Standard
        public ApprenticeshipStandardContentItemBuilder ApprenticeshipStandardContentItemBuilder { get; set; }

        public ApprenticeshipStandardContentItem ApprenticeshipStandardContentItems { get; set;}

        // Job Profiles
        //public IEnumerable<JobProfileContentItem> JobProfileContentItems { get; set; }

        // Job Category
        //public JobCategoryContentItem JobCategoryContentItems { get; set; }

        public ReferenceData Build(JobProfileSettingsDataModel jobProfileSettingsDataModel)
        {
            // The job profile data spreadsheet exported from Sitefinity
            JobProfileExcelWorkbook = GetExcelWorkbook(jobProfileSettingsDataModel.JobProfileExcelWorkbookPath); ;

            // The dysac data spreadsheet exported from Sitefinity
            DysacExcelWorkbook = GetExcelWorkbook(jobProfileSettingsDataModel.DysacExcelWorkbookPath); ;

            // The dysac/job profile mapping data spreadsheet
            DysacJobProfileMappingExcelWorkbook = GetExcelWorkbook(jobProfileSettingsDataModel.DysacJobProfileMappingExcelWorkbookPath);

            // The dysac/job profile mapping data spreadsheet
            SocSkillsMatricExcelWorkbook = GetExcelWorkbook(jobProfileSettingsDataModel.DysacJobProfileMappingExcelWorkbookPath);

            // SocCodes
            SocCodeContentItemBuilder = new SocCodeContentItemBuilder(jobProfileSettingsDataModel.SocCodeDictionary).Build(jobProfileSettingsDataModel.Timestamp);

            // JobCategory Builder
            JobCategoryBuilder = new JobCategoryBuilder(jobProfileSettingsDataModel, this).Build();

            // Onet Occupational Codes
            ONetOccupationalCodeContentItemBuilder = new ONetOccupationalCodeContentItemBuilder(jobProfileSettingsDataModel.OnetCodeDictionary).Build(JobProfileExcelWorkbook, jobProfileSettingsDataModel.Timestamp);

            // Onet Skills
            OnetSkillContentItemBuilder = new OnetSkillContentItemBuilder(jobProfileSettingsDataModel, this).Build();

            // QCF Levels
            QcfLevelContentItemBuilder = new QcfLevelContentItemBuilder().Build(jobProfileSettingsDataModel.Timestamp);

            // JobProfiles
            JobProfileContentItemBuilder = new JobProfileContentItemBuilder(jobProfileSettingsDataModel, this).Build();

            // Apprenticeship Standard Builder (dependent on JobProfileContentItemBuilder, order of building is important)
            ApprenticeshipStandardContentItemBuilder = new ApprenticeshipStandardContentItemBuilder(jobProfileSettingsDataModel.ApprenticeshipStandardsRefDictionary).Build(jobProfileSettingsDataModel, this);

            return this;
        }

        private XSSFWorkbook GetExcelWorkbook(string JobProfileWorkbookPath)
        {
            if (JobProfileWorkbookPath == null)
            {
                throw new ArgumentNullException("JobProfileWorkbookPath is null");
            }

            using var jobProfileWorkbookStreamReader = new StreamReader(JobProfileWorkbookPath);

            if (jobProfileWorkbookStreamReader == null)
            {
                throw new Exception("jobProfileWorkbookStreamReader is null");
            }

            var jobProfileWorkbook = new XSSFWorkbook(jobProfileWorkbookStreamReader.BaseStream);

            if (jobProfileWorkbookStreamReader == null)
            {
                throw new Exception("jobProfileSpreadSheet is null");
            }

            return jobProfileWorkbook;
        }
    }
}
