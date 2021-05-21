using System;
using System.Collections.Generic;
using System.IO;
using GetJobProfiles.Entities;
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
        public XSSFWorkbook SocSkillsMatrixExcelWorkbook { get; set; }

        // Onet skill
        //public OnetSkillContentItemBuilder OnetSkillContentItemBuilder { get; set; }

        // University Requirement
        // University Link
        // College Requirement
        // College Link
        // Apprencticeship Requirement
        // Apprenticeship Link


        // SOC Codes Builder
        public SocCode SocCodes { get; set; }

        // ONet Occupational Codes Builder
        public ONetOccCode ONetOccCodes { get; set; }

        // ONet Skills Builder
        public OnetSkill OnetSkills { get; set;}

        // QCF Levels Builder
        public QcfLevel QcfLevels { get; set; }

        // JobProfiles Builder
        public JobProfile JobProfiles { get; set; }

        // JobCategory Builder
        public JobCategory JobCategories { get; set; }

        // Apprenticeship Standard and Standard Route Builder
        public ApprenticeshipStandard ApprenticeshipStandards { get; set; }

        // Digital Skills Level Builder
        public DigitalSkillsLevel DigitalSkillsLevel { get; set; }

        // Onet Occupational Code Content Items
        public ONetOccupationalCodeContentItem ONetOccupationalCodeContentItems { get; set; }



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


        // Working Environment
        // Working Location
        // Working Uniform
        // Apprencticeship Standard


        // Job Profiles
        //public IEnumerable<JobProfileContentItem> JobProfileContentItems { get; set; }

        // Job Category
        //public JobCategoryContentItem JobCategoryContentItems { get; set; }

        public ReferenceData Build(SettingsModel settings)
        {
            // The job profile data spreadsheet exported from Sitefinity
            JobProfileExcelWorkbook = GetExcelWorkbook(settings.JobProfileExcelWorkbookPath);

            return this;

            // The dysac data spreadsheet exported from Sitefinity
            DysacExcelWorkbook = GetExcelWorkbook(settings.DysacExcelWorkbookPath);

            // The dysac/job profile mapping data spreadsheet
            DysacJobProfileMappingExcelWorkbook = GetExcelWorkbook(settings.DysacJobProfileMappingExcelWorkbookPath);

            // The dysac/job profile mapping data spreadsheet
            SocSkillsMatrixExcelWorkbook = GetExcelWorkbook(settings.DysacJobProfileMappingExcelWorkbookPath);

            // TODO: Uncomment after testing
            return this;

            // SocCodes
            SocCodes = new SocCode(settings.SocCodeDictionary).Build(settings.Timestamp);

            // JobCategory Builder
            JobCategories = new JobCategory(settings, this).Build();

            // Onet Occupational Codes
            ONetOccCodes = new ONetOccCode(settings.OnetCodeDictionary).Build(JobProfileExcelWorkbook, settings.Timestamp);

            // Onet Skills
            OnetSkills = new OnetSkill(settings, this).Build();

            // QCF Levels
            QcfLevels = new QcfLevel().Build(settings.Timestamp);

            // Digital Skills Level
            DigitalSkillsLevel = new DigitalSkillsLevel().Build(settings.Timestamp);

            // JobProfiles
            JobProfiles = new JobProfile(settings, this).Build(0, 0, 5000, settings.JobProfilesToImport);

            // Apprenticeship Standard Builder (dependent on JobProfileContentItemBuilder, order of building is important)
            ApprenticeshipStandards = new ApprenticeshipStandard(settings.ApprenticeshipStandardsRefDictionary).Build(settings, this);

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
