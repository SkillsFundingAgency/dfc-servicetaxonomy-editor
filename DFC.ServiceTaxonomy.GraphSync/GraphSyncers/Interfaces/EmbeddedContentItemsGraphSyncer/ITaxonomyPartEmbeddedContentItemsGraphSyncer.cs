namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer
{
    public interface ITaxonomyPartEmbeddedContentItemsGraphSyncer : IEmbeddedContentItemsGraphSyncer
    {
        bool IsNonLeafEmbeddedTerm { get; set; }
    }
}
