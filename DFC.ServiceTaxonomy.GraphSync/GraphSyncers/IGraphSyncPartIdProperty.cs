namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public interface IGraphSyncPartIdProperty
    {
        string Name { get; }

        string Value(dynamic graphSyncContent);
    }
}
