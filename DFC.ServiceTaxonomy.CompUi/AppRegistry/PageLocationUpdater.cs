using DFC.ServiceTaxonomy.CompUi.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.CompUi.AppRegistry
{
    public class PageLocationUpdater : IPageLocationUpdater
    {
        private const string ConnectionStringAppSetting = "CosmosAppRegistry:ConnectionString";
        private string ConnectionString;
        private const string DatabaseNameAppSettings = "CosmosAppRegistry:DatabaseName";
        private string DatabaseName;
        private const string ContainerNameAppSettings = "CosmosAppRegistry:ContainerName";
        private string ContainerName;
        private const string ContainerDraftNameAppSettings = "CosmosAppRegistry:ContainerDraftName";
        private string ContainerDraftName;
        private const string partitionKey = "pages";
        private const string DraftFilter = "DRAFT";
        private readonly ILogger<PageLocationUpdater> _logger;
        private readonly CosmosClient _client;
        private readonly Database _database;
        private Container _container;

        public PageLocationUpdater(ILogger<PageLocationUpdater> logger, IConfiguration configuration)
        {
            ConnectionString = configuration.GetSection(ConnectionStringAppSetting).Get<string>();
            DatabaseName = configuration.GetSection(DatabaseNameAppSettings).Get<string>();
            ContainerName = configuration.GetSection(ContainerNameAppSettings).Get<string>();
            ContainerDraftName = configuration.GetSection(ContainerDraftNameAppSettings).Get<string>();

            _client = new CosmosClient(ConnectionString, new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway });
            _database = _client.GetDatabase(DatabaseName);
            _logger = logger;
        }

        public async Task<ContentPageModel> GetPageById(string filter)
        {
            List<ContentPageModel?> appRegistry = new List<ContentPageModel?>();
            _container = GetContainer(filter);
            try
            {
                using (FeedIterator<ContentPageModel> appRegistryIterator = _container.GetItemLinqQueryable<ContentPageModel>()
               .Where(m => m.PartitionKey == partitionKey)
               .ToFeedIterator())
                {
                    while (appRegistryIterator.HasMoreResults)
                    {
                        foreach (var item in await appRegistryIterator.ReadNextAsync())
                        {
                            appRegistry.Add(item);
                        }
                    }
                }
                return appRegistry.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception when retrieving AppRegistry Pages: {ex}");
                throw;
            }
        }

        public async Task<ContentPageModel> UpdatePages(string nodeId, List<string> locations, string filter)
        {
            var items = await GetPageById(filter);

            _container = GetContainer(filter);

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
                    return await _container.UpsertItemAsync<ContentPageModel>(items, new PartitionKey(items.PartitionKey));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception when Updating AppRegistry Pages for {nodeId}: {ex}");
                throw;
            }
        }

        public async Task<ContentPageModel> DeletePages(string nodeId, string filter)
        {
            var items = await GetPageById(filter);
            _container = GetContainer(filter);


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
                _logger.LogError(ex, $"Exception when Deleting AppRegistry Page for {nodeId}: {ex}");
                throw;
            }
        }

        public Container GetContainer(string filter)
        {
            if (filter == DraftFilter)
            {
                _container = _database.GetContainer(ContainerDraftName);
                return _container;
            }
            else
            {
                _container = _database.GetContainer(ContainerName);
                return _container;
            }
        }
    }
}
