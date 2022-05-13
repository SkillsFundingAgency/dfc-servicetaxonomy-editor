using GetJobProfiles.Models.Recipe.Fields;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class JobCategoryPart
    {
        public TaxonomyField PageLocations { get; set; }
        public HtmlField Description { get; set; }
        public ContentPicker JobProfiles { get; set; }
    }
}
