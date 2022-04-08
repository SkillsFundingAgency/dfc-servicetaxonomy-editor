namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    //
    // Summary:
    //     Provides information about the database that processed the query.
    public interface IDatabaseInfo
    {
        //
        // Summary:
        //     The name of the database where the query is processed.
        //
        // Remarks:
        //     Returns
        //     null
        //     if the source server does not support multiple databases.
        string Name { get; }
    }
}
