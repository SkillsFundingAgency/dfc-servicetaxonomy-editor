using System;
using System.Collections.Generic;
using System.Text;

namespace GetJobProfiles.Models.API
{
    public class DysacTrait
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> JobCategories { get; set; }
    }
}
