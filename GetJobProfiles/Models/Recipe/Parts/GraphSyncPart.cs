using System;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class GraphSyncPart
    {
        public GraphSyncPart(string contentType) => Text = $"http://nationalcareers.service.gov.uk/{contentType.ToLowerInvariant()}/{Guid.NewGuid()}";

        public string Text { get; set; }
    }
}
