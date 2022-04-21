namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.EmbeddedContentItemsDataSyncer
{
    public interface ITaxonomyPartEmbeddedContentItemsDataSyncer : IEmbeddedContentItemsDataSyncer
    {
        bool IsNonLeafEmbeddedTerm { get; set; }
    }
}
