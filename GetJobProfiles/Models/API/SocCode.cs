using GetJobProfiles.Models.Recipe;

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
        // {
        //     ContentItemId = generator.GenerateUniqueId(),
        //     ContentItemVersionId = generator.GenerateUniqueId(),
        //     ContentType = "SOCCode",
        //     DisplayText = Unit,
        //     Latest = true,
        //     Published = true,
        //     ModifiedUtc = $"{DateTime.UtcNow:O}",        //todo: global
        //     PublishedUtc = $"{DateTime.UtcNow:O}",
        //     CreatedUtc = $"{DateTime.UtcNow:O}",
        //     Owner = "[js: parameters('AdminUsername')]",
        //     Author = "[js: parameters('AdminUsername')]",
        //     GraphSyncPart = new GraphSyncPart($"http://nationalcareers.service.gov.uk/soccode/{Guid.NewGuid()}"),
        //     TitlePart = new TitlePart(Unit),
        //     Description = new TextField(Title)
        // };
    }
}
