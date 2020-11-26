using GetJobProfiles.Models.Recipe.Fields;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class PersonalityTraitPart
    {
        public TextField Description { get; set; }
        public ContentPicker JobCategories { get; set; }
    }
}
