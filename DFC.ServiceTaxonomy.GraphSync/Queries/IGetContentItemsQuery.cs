using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Newtonsoft.Json.Linq;

//using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.Queries
{
    interface IGetContentItemsQuery : IQuery<List<JObject>>
    {
        public string? QueryStatement { get; set; }
    }
}
