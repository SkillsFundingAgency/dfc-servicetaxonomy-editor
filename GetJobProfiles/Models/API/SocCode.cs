using GetJobProfiles.Models.Recipe.ContentItems;

namespace GetJobProfiles.Models.API
{
    public class SocCode
    {
        // public string Major { get; set; }
        // public string Sub { get; set; }
        // public string Minor { get; set; }
        public string Unit { get; set; }
        public string Title { get; set; }

        public SocCodeContentItem ToContentItem(string timestamp) =>
            new SocCodeContentItem(Unit, timestamp, Title);
    }
}
