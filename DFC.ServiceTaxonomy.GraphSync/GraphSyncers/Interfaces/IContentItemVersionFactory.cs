namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IContentItemVersionFactory
    {
        IContentItemVersion Get(string graphReplicaSetName);
    }
}
