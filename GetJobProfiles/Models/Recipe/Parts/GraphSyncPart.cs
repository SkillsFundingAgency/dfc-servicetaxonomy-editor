using System;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;

namespace GetJobProfiles.Models.Recipe.Parts
{
    public class GraphSyncPart
    {
        public GraphSyncPart(string contentType) => Text = $"{GraphSyncHelper.ContentApiPrefixToken}/{contentType.ToLowerInvariant()}/{Guid.NewGuid()}";

        public string Text { get; set; }
    }
}
