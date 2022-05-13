using GetJobProfiles.Models.Recipe.Fields;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class PersonalityQuestionSetPart
    {
        public TextField Type { get; set; }
        public ContentPicker Questions { get; set; }
    }
}
