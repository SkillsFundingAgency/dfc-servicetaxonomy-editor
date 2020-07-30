namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer
{
    public interface ITaxonomyPartEmbeddedContentItemsGraphSyncer : IEmbeddedContentItemsGraphSyncer
    {
        bool IsRoot { get; set; }
    }
}
