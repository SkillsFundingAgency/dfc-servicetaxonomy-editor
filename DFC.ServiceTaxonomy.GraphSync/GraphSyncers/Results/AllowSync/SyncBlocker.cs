using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync
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
