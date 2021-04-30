using System.Collections.Generic;
using GetJobProfiles.Models.Containers;

namespace GetJobProfiles.Models.Recipe.Fields.Factories
{
    public class DigitalSkillsLevelContentPickerFactory
    {
        private readonly Dictionary<string, JobProfileExcelWorkbookColumnsDataModel> _digitalSkillsLevelDictionary;

        public DigitalSkillsLevelContentPickerFactory(Dictionary<string, JobProfileExcelWorkbookColumnsDataModel> digitalSkillsLevelDictionary)
        {
            _digitalSkillsLevelDictionary = digitalSkillsLevelDictionary;
        }

        public ContentPicker Create(string digitalSkillsLevelContentPicker)
        {
            if (!_digitalSkillsLevelDictionary.TryGetValue(digitalSkillsLevelContentPicker, out JobProfileExcelWorkbookColumnsDataModel contentItemId))
            {
                contentItemId = new JobProfileExcelWorkbookColumnsDataModel();
                contentItemId.DigitalSkillsLevel = "undefined";
            }

            return new ContentPicker {ContentItemIds = new[] {contentItemId.DigitalSkillsLevel}};
        }
    }
}
