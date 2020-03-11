using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.Queries
{
    interface IGetContentItemsQuery : IQuery<ContentItem>
    {
    }
}
