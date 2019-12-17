namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    //todo: better name
    public class GraphSyncPartIdProperty : IGraphSyncPartIdProperty
    {
        //todo: from settings
        public string Name => "uri";

        public string Value(dynamic graphSyncContent) => graphSyncContent.Text.ToString();
    }
}
