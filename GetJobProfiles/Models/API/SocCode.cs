using System;
using GetJobProfiles.Models.Recipe;

namespace GetJobProfiles.Models.API
{
    public class SocCode
    {
        public string Major { get; set; }
        public string Sub { get; set; }
        public string Minor { get; set; }
        public string Unit { get; set; }
        public string Title { get; set; }

        public SocCodeContentItem ToContentItem() => new SocCodeContentItem
        {
            ContentItemId = "[js:uuid()]",
            ContentItemVersionId = "[js:uuid()]",
            ContentType = "SOCCode",
            DisplayText = Unit,
            Latest = true,
            Published = true,
            ModifiedUtc = $"{DateTime.UtcNow:O}",
            PublishedUtc = $"{DateTime.UtcNow:O}",
            CreatedUtc = $"{DateTime.UtcNow:O}",
            Owner = "[js: parameters('AdminUsername')]",
            Author = "[js: parameters('AdminUsername')]",
            GraphSyncPart = new GraphSyncPart
            {
                Text = "???" //not sure what this needs to be
            },
            TitlePart = new TitlePart
            {
                Title = Unit
            },
            Description = new TextField
            {
                Text = Title
            }
        };
    }
}
