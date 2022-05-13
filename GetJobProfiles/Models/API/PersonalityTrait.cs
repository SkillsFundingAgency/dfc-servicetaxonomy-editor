using System.Collections.Generic;

namespace GetJobProfiles.Models.API
{
    public class PersonalityTrait
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> JobCategories { get; set; }
    }
}
