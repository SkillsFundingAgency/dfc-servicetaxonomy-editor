using System;
using System.Collections.Generic;
using System.Text;
using GetJobProfiles.Models.Recipe.Fields;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class ApprenticeshipStandardPart
    {
        public HtmlField Description { get; set; }
        public TextField Reference { get; set; }
        public NumericField MaximumFunding { get; set; }
        public NumericField LARSCode { get; set; }
        public NumericField Duration { get; set; }
        public ContentPicker ApprenticeshipStandardRoutes { get; set; }
        public ContentPicker QCFLevel { get; set; }
        public TextField Type { get; set; }
    }
}
