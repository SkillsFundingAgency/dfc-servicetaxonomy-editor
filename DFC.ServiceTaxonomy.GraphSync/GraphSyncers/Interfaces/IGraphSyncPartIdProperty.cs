namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IGraphSyncPartIdProperty
    {
        string Name { get; }

        object Value(dynamic graphSyncContent);
    }
}
