namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions
{
    public interface IContentItemVersionFactory
    {
        IContentItemVersion Get(string dataSyncReplicaSetName);
    }
}
