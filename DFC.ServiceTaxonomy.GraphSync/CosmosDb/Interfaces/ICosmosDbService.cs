using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb.Interfaces
{
    public interface ICosmosDbService
    {
        Container GetContainer(string name);
        Task DeleteIncomingRelationshipAsync(Container container, string contentType, string id, string relationshipId);

        Task<Dictionary<string, object>?> GetContentItemFromDatabase(Container container, string contentType, string id);
    }
}
