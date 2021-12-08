namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    //
    // Summary:
    //     Provides basic information of the server where the cypher query was executed.
    public interface IServerInfo
    {
        //
        // Summary:
        //     Get the address of the server
        string Address { get; }

        //
        // Summary:
        //     Get the version of Neo4j running at the server.
        //
        // Remarks:
        //     Introduced since Neo4j 3.1. Default to null if not supported by server
        string Version { get; }
    }
}
