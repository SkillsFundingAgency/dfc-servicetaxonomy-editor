using System;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.AzureSearchIndexHandling.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.AzureSearch;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.AzureSearchIndexHandling
{
    public class AzureSearchDataProcessor<T> : IAzureSearchDataProcessor<T>
    {
        private readonly ILogger<AzureSearchDataProcessor<T>> _logger;
        private readonly IMessageConverter<JobProfileIndex> _jobProfileIndexConverter;
        private readonly IConfiguration _configuration;

        public AzureSearchDataProcessor(ILogger<AzureSearchDataProcessor<T>> logger, IMessageConverter<JobProfileIndex> jobProfileIndexConverter, IConfiguration configuration)
        {
            _logger = logger;
            _jobProfileIndexConverter = jobProfileIndexConverter;
            _configuration = configuration;

        }

        public async Task<T> ProcessContentContext(ContentContextBase context, string actionType)
        {
            try
            {
                var jobProfileIndex = await _jobProfileIndexConverter.ConvertFromAsync(context.ContentItem);
                var jobProfileIndexSettings = _configuration.GetSection("AzureSearchSettings").Get<JobProfileIndexSettings>();

                SearchIndexClient indexClient = CreateSearchIndexClient(jobProfileIndexSettings.SearchServiceEndPoint?? string.Empty, jobProfileIndexSettings.SearchServiceAdminAPIKey?? string.Empty);
                SearchClient searchClient = indexClient.GetSearchClient(jobProfileIndexSettings.JobProfileSearchIndexName);
                IndexDocumentsBatch<JobProfileIndex> jobProfileIndexBatch = IndexDocumentsBatch.Create(IndexDocumentsAction.MergeOrUpload(jobProfileIndex));
                IndexDocumentsOptions options = new IndexDocumentsOptions { ThrowOnAnyError = true };
                var jsonObject = Newtonsoft.Json.JsonConvert.SerializeObject(jobProfileIndex);
                IndexDocumentsResult  result = searchClient.IndexDocuments(jobProfileIndexBatch, options);
                foreach(var item in result.Results)
                {
                    Console.WriteLine($"{jsonObject} - {item.Succeeded} TODO: remove Me");
                }

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Failed to export data for item with ContentItemId = {context.ContentItem.ContentItemId}");
                throw;
            }

            throw new NotImplementedException();
        }

        private static SearchIndexClient CreateSearchIndexClient(string searchServiceEndPoint, string adminApiKey)
        {
            SearchIndexClient indexClient = new SearchIndexClient(new Uri(searchServiceEndPoint), new AzureKeyCredential(adminApiKey));
            return indexClient;
        }

    }
}
