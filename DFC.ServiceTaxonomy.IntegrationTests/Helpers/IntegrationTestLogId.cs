using DFC.ServiceTaxonomy.Neo4j.Log;

namespace DFC.ServiceTaxonomy.IntegrationTests.Helpers
{
    public enum IntegrationTestLogId
    {
        RunQueryStarted = DFC.ServiceTaxonomy.Neo4j.Log.LogId.IntegrationTestsStart,
        RunQueryFinished
    }
}
