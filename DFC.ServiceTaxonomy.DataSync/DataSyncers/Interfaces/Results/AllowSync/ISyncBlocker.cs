namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync
{
    public interface ISyncBlocker
    {
        string ContentType { get; }
        string? Title { get; }
        object Id { get; }
    }
}
