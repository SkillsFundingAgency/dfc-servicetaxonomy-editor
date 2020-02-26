namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public interface IGraphSyncPartIdProperty
    {
        string Name { get; }

        object Value(dynamic graphSyncContent);
    }
}
