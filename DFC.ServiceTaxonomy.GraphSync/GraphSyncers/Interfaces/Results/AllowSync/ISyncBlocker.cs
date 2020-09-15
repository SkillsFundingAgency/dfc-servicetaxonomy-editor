namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync
{
    public interface ISyncBlocker
    {
        string ContentType { get; }
        string? Title { get; }
        object Id { get; }
    }
}
