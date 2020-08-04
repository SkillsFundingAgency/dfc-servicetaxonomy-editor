using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries
{
    interface IGetContentItemsAsJsonQuery : IQuery<string>
    {
        public string? QueryStatement { get; set; }
    }
}
