using System.Collections.Generic;
using GetJobProfiles.Models.Containers;

namespace GetJobProfiles.Models.Recipe.Fields.Factories
{
    public class DigitalSkillsLevelContentPickerFactory
    {
        private readonly Dictionary<string, JobProfileExcelWorkbookColumnsModel> _digitalSkillsLevelDictionary;

        public DigitalSkillsLevelContentPickerFactory(Dictionary<string, JobProfileExcelWorkbookColumnsModel> digitalSkillsLevelDictionary)
        {
            _digitalSkillsLevelDictionary = digitalSkillsLevelDictionary;
        }

        public ContentPicker Create(string digitalSkillsLevelContentPicker)
        {
            if (!_digitalSkillsLevelDictionary.TryGetValue(digitalSkillsLevelContentPicker, out JobProfileExcelWorkbookColumnsModel contentItemId))
            {
                contentItemId = new JobProfileExcelWorkbookColumnsModel();
                contentItemId.DigitalSkillsLevel = "undefined";
            }

            return new ContentPicker {ContentItemIds = new[] {contentItemId.DigitalSkillsLevel}};
        }
    }
}
