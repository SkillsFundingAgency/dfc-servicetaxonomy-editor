namespace DFC.ServiceTaxonomy.GraphSync.Enums
{
    //
    // Summary:
    //     The type of a query.
    public enum QueryType
    {
        //
        // Summary:
        //     The query type is unknown
        Unknown = 0,
        //
        // Summary:
        //     The query is a readonly query
        ReadOnly = 1,
        //
        // Summary:
        //     The query is a readwrite query
        ReadWrite = 2,
        //
        // Summary:
        //     The query is a writeonly query
        WriteOnly = 3,
        //
        // Summary:
        //     The query is a schemawrite query
        SchemaWrite = 4
    }
}
