namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface ISyncBlocker
    {
        string ContentType { get; }
        string? Title { get; }
    }
}
