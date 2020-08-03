using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public class SyncBlocker : ISyncBlocker
    {
        public string ContentType { get; }
        public string? Title { get; }

        public SyncBlocker(string contentType, string? title)
        {
            ContentType = contentType;
            Title = title;
        }
    }
}
