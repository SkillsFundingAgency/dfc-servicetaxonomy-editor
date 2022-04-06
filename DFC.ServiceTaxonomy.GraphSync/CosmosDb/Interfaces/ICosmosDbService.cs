using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb.Interfaces
{
    public interface ICosmosDbService
    {
        Task DeleteIncomingRelationshipAsync(string databaseName, string contentType, Guid id, string relationshipId);
        Task DeleteItemAsync(string databaseName, string contentType, Guid id);
        Task UpdateItemAsync(string databaseName, Dictionary<string, object> item);
        Task<Dictionary<string, object>?> GetContentItemFromDatabase(string databaseName, string contentType, Guid id);
        Task<List<T>> QueryContentItemsAsync<T>(string databaseName, string query, string contentType);
    }
}
