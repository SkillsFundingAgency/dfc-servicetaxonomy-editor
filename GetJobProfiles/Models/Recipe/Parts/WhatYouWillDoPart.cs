using GetJobProfiles.Models.Recipe.Fields;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class WhatYouWillDoPart
    {
        public TabField WhatYouWillDo { get; set; } = new TabField();
        public ContentPicker DayToDayTasks { get; set; }
        public ContentPicker WydWorkingEnvironment { get; set; }
        public ContentPicker WydWorkingLocation { get; set; }
        public ContentPicker WydWorkingUniform { get; set; }
    }
}
