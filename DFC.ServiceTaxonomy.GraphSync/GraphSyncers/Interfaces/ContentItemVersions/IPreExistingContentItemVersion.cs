namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions
{
    public interface IPreExistingContentItemVersion : IContentItemVersion
    {
        void SetContentApiBaseUrl(string? contentApiBaseUrl);
    }
}
