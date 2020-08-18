namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions
{
    public interface IContentItemVersionFactory
    {
        IContentItemVersion Get(string graphReplicaSetName);
    }
}
