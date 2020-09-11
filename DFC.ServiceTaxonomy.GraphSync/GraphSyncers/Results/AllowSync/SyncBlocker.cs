using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync
{
    public class SyncBlocker : ISyncBlocker
    {
        public string ContentType { get; }
        public object Id { get; }
        public string? Title { get; }

        public SyncBlocker(string contentType, object id, string? title)
        {
            ContentType = contentType;
            Id = id;
            Title = title;
        }

        public override string ToString()
        {
            return $"'{Title ?? "n/a"}' {ContentType}";
        }
    }
}
