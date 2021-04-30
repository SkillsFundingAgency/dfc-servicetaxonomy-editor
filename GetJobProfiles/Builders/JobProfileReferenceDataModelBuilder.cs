using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Recipes.Executors;
using GetJobProfiles.Builders;
using GetJobProfiles.JsonHelpers;
using GetJobProfiles.Models.Containers;
using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Factories;
using GetJobProfiles.Models.Recipe.Fields;
using Microsoft.Extensions.Configuration;
using MoreLinq;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles.Builders
{
    public class JobProfileReferenceDataModelBuilder
    {
        public ReferenceData JobProfileReferenceDataModel
        {
            get;
            private set;
        } = new ReferenceData();

        public JobProfileReferenceDataModelBuilder Build(JobProfileSettingsDataModel jobProfileSettingsDataModel)
        {
            // The job profile data spreadsheet exported from Sitefinity
            JobProfileReferenceDataModel.JobProfileExcelWorkbook = GetExcelWorkbook(jobProfileSettingsDataModel.JobProfileExcelWorkbookPath); ;

            // The dysac data spreadsheet exported from Sitefinity
            JobProfileReferenceDataModel.DysacExcelWorkbook = GetExcelWorkbook(jobProfileSettingsDataModel.DysacExcelWorkbookPath); ;

            // The dysac/job profile mapping data spreadsheet
            JobProfileReferenceDataModel.DysacJobProfileMappingExcelWorkbook = GetExcelWorkbook(jobProfileSettingsDataModel.DysacJobProfileMappingExcelWorkbookPath);

            // The dysac/job profile mapping data spreadsheet
            JobProfileReferenceDataModel.SocSkillsMatricExcelWorkbook = GetExcelWorkbook(jobProfileSettingsDataModel.DysacJobProfileMappingExcelWorkbookPath);

            // SocCodes
            JobProfileReferenceDataModel.SocCodeContentItemBuilder = new SocCodeContentItemBuilder(jobProfileSettingsDataModel.SocCodeDictionary).Build(jobProfileSettingsDataModel.Timestamp);

            // Onet Occupational Codes
            JobProfileReferenceDataModel.ONetOccupationalCodeContentItemBuilder = new ONetOccupationalCodeContentItemBuilder(jobProfileSettingsDataModel.OnetCodeDictionary).Build(JobProfileReferenceDataModel.JobProfileExcelWorkbook, jobProfileSettingsDataModel.Timestamp);

            // Onet Skills
            JobProfileReferenceDataModel.OnetSkillContentItemBuilder = new OnetSkillContentItemBuilder(jobProfileSettingsDataModel, JobProfileReferenceDataModel).Build();

            // QCF Levels
            JobProfileReferenceDataModel.QcfLevelContentItemBuilder = new QcfLevelContentItemBuilder().Build(jobProfileSettingsDataModel.Timestamp);

            // JobProfiles
            JobProfileReferenceDataModel.JobProfileContentItemBuilder = new JobProfileContentItemBuilder(jobProfileSettingsDataModel, JobProfileReferenceDataModel).Build();

            // Apprenticeship Standard Builder (dependent on JobProfileContentItemBuilder, order of building is important)
            JobProfileReferenceDataModel.ApprenticeshipStandardContentItemBuilder = new ApprenticeshipStandardContentItemBuilder(jobProfileSettingsDataModel.ApprenticeshipStandardsRefDictionary).Build(jobProfileSettingsDataModel, JobProfileReferenceDataModel);

            // JobCategory Builder
            JobProfileReferenceDataModel.JobCategoryBuilder = new JobCategoryBuilder(jobProfileSettingsDataModel, JobProfileReferenceDataModel).Build();

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
