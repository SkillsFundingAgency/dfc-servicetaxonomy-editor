using GetJobProfiles.Models.Recipe.Fields;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class JobCategoryPart
    {
        public HtmlField Description { get; set; }
        public TextField WebsiteURI { get; set; }
        public ContentPicker JobProfiles { get; set; }
    }
}
