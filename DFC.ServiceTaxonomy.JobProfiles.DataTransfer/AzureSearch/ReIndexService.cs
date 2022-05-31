using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.AzureSearch;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;

using YesSql;

using static DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch.SearchClientHelper;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch
{
    public class ReIndexService : IReIndexService
    {
        private readonly JobProfileIndexSettings _jobProfileIndexSettings;
        private readonly ILogger<ReIndexService> _logger;
        private readonly IServiceProvider _provider;
        private readonly IMessageConverter<JobProfileIndex> _jobProfileIndexConverter;

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public ReIndexService(Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<ReIndexService> logger, IServiceProvider provider, IMessageConverter<JobProfileIndex> jobProfileIndexConverter)
        {

            _jobProfileIndexSettings = configuration.GetSection("AzureSearchSettings").Get<JobProfileIndexSettings>()
                ?? throw new ArgumentNullException(nameof(configuration), $"{nameof(JobProfileIndexSettings)} configuration is invalid");
            _logger = logger;
            _provider = provider;
            _jobProfileIndexConverter = jobProfileIndexConverter;
        }

        public async Task ReIndexAsync()
        {
            if (!await _semaphore.WaitAsync(TimeSpan.Zero))
            {
                return;
            }

            try
            {
                var session = _provider.GetRequiredService<ISession>();
                var jpContentItems = await session.Query<ContentItem, ContentItemIndex>(c => c.ContentType == ContentTypes.JobProfile && c.Published && c.Latest).ListAsync();
                SearchClient client = await GetSearchClientAsync(_jobProfileIndexSettings);

                foreach (var item in jpContentItems)
                {
                    try
                    {
                        var jobProfileIndexDocument = await _jobProfileIndexConverter.ConvertFromAsync(item);

                        var documentUploadResponse = await UploadDocumentAsync(client, jobProfileIndexDocument);
                        if (IsSuccessful(documentUploadResponse))
                        {
                            _logger.Log(LogLevel.Information, $"Job Profile index document - {jobProfileIndexDocument.IdentityField} successfully uploaded to Azure Search Index");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Job Profile index document failed to upload for Job Profile ContentItemId = {item.ContentItemId}");
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<SearchClient> GetSearchClientAsync(JobProfileIndexSettings jobProfileIndexSettings)
        {
            var searchServiceEndPoint = jobProfileIndexSettings.SearchServiceEndPoint ?? string.Empty;
            var adminApiKey = jobProfileIndexSettings.SearchServiceAdminAPIKey ?? string.Empty;

            SearchIndexClient indexClient = new SearchIndexClient(new Uri(searchServiceEndPoint), new AzureKeyCredential(adminApiKey));
            var jobProfileIndexName = _jobProfileIndexSettings.JobProfileSearchIndexName ?? string.Empty;
            if (await indexClient.GetIndexesAsync().AnyAsync(index => index.Name == jobProfileIndexName))
            {
                // Delete index if exist
                await indexClient.DeleteIndexAsync(jobProfileIndexName);
            }

            SearchIndex definition = GetSearchIndexDefinition(jobProfileIndexName);
            await indexClient.CreateIndexAsync(definition);

            return indexClient.GetSearchClient(jobProfileIndexName);
        }
    }
}
