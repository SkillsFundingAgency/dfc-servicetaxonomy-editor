using Microsoft.Extensions.Configuration;

namespace GetJobProfiles.Models.Containers
{
    public class SettingsModel
    {
        public string RecipeOutputBasePath { get; set; }

        public string MasterRecipeOutputBasePath { get; set; }

        public string JobProfileExcelWorkbookPath { get; set; }

        public string DysacExcelWorkbookPath { get; set; }

        public string DysacJobProfileMappingExcelWorkbookPath { get; set; }

        public string AppSettingsFilename { get; set; }

        public string JobProfileApiUri { get; set; }

        public bool Zip { get; set; }

        public int FileIndex { get; set; }

        public IConfiguration AppSettings { get; set; }

        public string JobProfilesToImport { get; set; }

        public string MasterRecipeName { get; set; }

        public bool CreateTestFilesBool { get; set; }

        public string[] SocCodeDictionary { get; set; }

        public string[] OnetCodeDictionary { get; set; }

        public string[] ApprenticeshipStandardsRefDictionary { get; set; }

        public string FilenamePrefix { get; set; }

        public int RecipeBatchSize {get; set;}

        public int JobProfileBatchSize { get; set; }

        public string Timestamp { get; set; }
    }
}
