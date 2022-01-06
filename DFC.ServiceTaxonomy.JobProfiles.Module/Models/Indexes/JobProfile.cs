using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Models.Indexes
{
    public class JobProfile : ContentPart
    {
        public IList<string>? Dynamictitleprefix { get; set; }
        public IList<string>? Jobprofilespecialism { get; set; }
        public IList<string>? Jobprofilecategory { get; set; }
        public IList<string>? Relatedcareerprofiles { get; set; }
        public IList<string>? SOCcode { get; set; }
        public IList<string>? HiddenAlternativeTitle { get; set; }
        public IList<string>? WorkingHoursDetails { get; set; }
        public IList<string>? Workingpattern { get; set; }
        public IList<string>? Workingpatterndetails { get; set; }
        public IList<string>? Universityentryrequirements { get; set; }
        public IList<string>? Relateduniversityrequirements { get; set; }
        public IList<string>? Relateduniversitylinks { get; set; }
        public IList<string>? Collegeentryrequirements { get; set; }
        public IList<string>? Relatedcollegerequirements { get; set; }
        public IList<string>? Relatedcollegelinks { get; set; }
        public IList<string>? Apprenticeshipentryrequirements { get; set; }
        public IList<string>? Relatedapprenticeshiprequirements { get; set; }
        public IList<string>? Relatedapprenticeshiplinks { get; set; }
        public IList<string>? Relatedregistrations { get; set; }
        public IList<string>? Digitalskills { get; set; }
        public IList<string>? Relatedlocations { get; set; }
        public IList<string>? Relatedenvironments { get; set; }
        public IList<string>? Relateduniforms { get; set; }
    }
}
