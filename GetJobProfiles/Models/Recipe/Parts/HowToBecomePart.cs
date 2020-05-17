using GetJobProfiles.Models.Recipe.Fields;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class HowToBecomePart
    {
        public TabField HowToBecome { get; set; }
        public HtmlField HtbBodies { get; set; }
        //todo: this field
        public TextField HtbTitleOptions { get; set; } = new TextField("as_defined");
        // public HtmlField HtbOtherRequirements { get; set; }
        public HtmlField HtbCareerTips { get; set; }
        public HtmlField HtbFurtherInformation { get; set; }
        public ContentPicker HtbRegistrations { get; set; }
    }
}
