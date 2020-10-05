using GetJobProfiles.Models.Recipe.Fields;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class WhatItTakesPart
    {
        public TabField WhatItTakes { get; set; }
        public HtmlField WitDigitalSkillsLevel { get; set; }
        public ContentPicker WitRestrictions { get; set; }
        public ContentPicker WitOtherRequirements { get; set; }
        public ContentPicker PersonalitySkills { get; set; }
    }
}
