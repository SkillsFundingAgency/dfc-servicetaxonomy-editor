using System;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class GraphSyncPart
    {
        public GraphSyncPart(string contentType) => Text = $"[js:configuration('ContentApiPrefix') + '/{contentType.ToLowerInvariant()}/{Guid.NewGuid()}']";

        public string Text { get; set; }
    }
}
