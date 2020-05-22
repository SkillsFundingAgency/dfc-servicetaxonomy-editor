using GetJobProfiles.Models.Recipe.Fields;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class HowToBecomePart
    {
        public TabField HowToBecome { get; set; }
        public ContentPicker ApprenticeshipRoute { get; set; }
        public ContentPicker CollegeRoute { get; set; }
        public ContentPicker UniversityRoute { get; set; }
        public ContentPicker DirectRoute { get; set; }
        public ContentPicker OtherRoute { get; set; }
        public ContentPicker VolunteeringRoute { get; set; }
        public ContentPicker WorkRoute { get; set; }
        public HtmlField HtbBodies { get; set; }
        // public HtmlField HtbOtherRequirements { get; set; }
        public HtmlField HtbCareerTips { get; set; }
        public HtmlField HtbFurtherInformation { get; set; }
        public ContentPicker HtbRegistrations { get; set; }
    }
}
