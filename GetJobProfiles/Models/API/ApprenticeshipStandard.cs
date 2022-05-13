using System;
using System.Collections.Generic;
using System.Text;

namespace GetJobProfiles.Models.API
{
    public class ApprenticeshipStandard
    {
        public string Name { get; set; }
        public string Reference { get; set; }
        public int? Level { get; set; }
        public int LARSCode { get; set; }
        public int MaximumFunding { get; set; }
        public int Duration { get; set; }
        public string[] Route { get; set; }
        public string Type { get; set; }
    }
}
