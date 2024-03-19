using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using DFC.Content.Pkg.Netcore.Data.Contracts;
using DFC.ServiceTaxonomy.CompUi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace DFC.ServiceTaxonomy.CompUi.AppRegistry
{
    public class PageLocationUpdater : IPageLocationUpdater
    {
        private const string EndpointUrl = "https://localhost:8081";
        private const string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const string DatabaseName = "composition";
        private const string ContainerName = "appregistry";
        private const string itemId = "7f7f8ae6-eb03-4ef1-8c09-b33548659893";
        private const string partitionKey = "pages";
        private readonly CosmosClient _client;
        private readonly Database _database;
        private readonly Container _container;

        public PageLocationUpdater()
        {
            _client = new CosmosClient(EndpointUrl, PrimaryKey, new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway });
            _database = _client.GetDatabase(DatabaseName);
            _container = _database.GetContainer(ContainerName);
            
        }

        public async Task<ContentPageModel> GetPageById()
        {   
            try
            {
                ItemResponse<ContentPageModel> response = await _container.ReadItemAsync<ContentPageModel>(itemId, new PartitionKey(partitionKey));                
                return response.Resource;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "failed");
            }
        }

        public async Task<ContentPageModel> UpdatePages(string nodeId, List<string> locations)
        {
            var items = await GetPageById();

            if (items.PageLocations.Keys.Any(t => t.Contains(nodeId)))
            {
                items.PageLocations.Remove(nodeId);
                items.PageLocations.Add(nodeId, new PageLocations { Locations = locations });
            }
            else
            {
                items.PageLocations.Add(nodeId, new PageLocations { Locations = locations });
            }

            try
            {
                ItemResponse<ContentPageModel> response = await _container.UpsertItemAsync<ContentPageModel>(items, new PartitionKey(items.PartitionKey));
                return response;

            }
            catch (Exception ex)
            {

                throw new Exception(
                    "failed");
            }
        }

        public async Task<ContentPageModel> DeletePages(string nodeId)
        {
            var items = await GetPageById();

            if (items.PageLocations.Keys.Any(t => t.Contains(nodeId)))
            {
                items.PageLocations.Remove(nodeId);
            }

            try
            {
                ItemResponse<ContentPageModel> response = await _container.UpsertItemAsync<ContentPageModel>(items, new PartitionKey(items.PartitionKey));
                return response;

            }
            catch (Exception ex)
            {

                throw new Exception(
                    "failed");
            }
        }
    }
}
