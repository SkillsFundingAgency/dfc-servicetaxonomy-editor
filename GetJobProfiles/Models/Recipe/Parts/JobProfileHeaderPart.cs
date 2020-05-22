using GetJobProfiles.Models.Recipe.Fields;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class JobProfileHeaderPart
    {
        public ContentPicker Occupation { get; set; }
        public HtmlField Description { get; set; }
        public TextField TitleOptions { get; set; }
        public TextField JobProfileWebsiteUrl { get; set; }
        public ContentPicker SOCCode { get; set; }
        public ContentPicker ONetOccupationalCode { get; set; }
        public TextField SalaryStarter { get; set; }
        public TextField SalaryExperienced { get; set; }
        public NumericField MinimumHours { get; set; }
        public NumericField MaximumHours { get; set; }
        public TextField WorkingHoursDetails { get; set; }
        public TextField WorkingPattern { get; set; }
        public TextField WorkingPatternDetails { get; set; }
    }
}
