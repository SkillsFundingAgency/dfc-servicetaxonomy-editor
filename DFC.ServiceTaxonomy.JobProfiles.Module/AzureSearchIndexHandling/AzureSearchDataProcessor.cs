using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.AzureSearchIndexHandling.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.AzureSearch;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.AzureSearchIndexHandling
{
    public class AzureSearchDataProcessor : IAzureSearchDataProcessor
    {
        private readonly ILogger<AzureSearchDataProcessor> _logger;
        private readonly IMessageConverter<JobProfileIndex> _jobProfileIndexConverter;
        private readonly IConfiguration _configuration;

        public AzureSearchDataProcessor(ILogger<AzureSearchDataProcessor> logger, IMessageConverter<JobProfileIndex> jobProfileIndexConverter, IConfiguration configuration)
        {
            _logger = logger;
            _jobProfileIndexConverter = jobProfileIndexConverter;
            _configuration = configuration;

        }

        public async Task ProcessContentContext(ContentContextBase context, string actionType)
        {
            try
            {
                var jobProfileIndexDocument = await _jobProfileIndexConverter.ConvertFromAsync(context.ContentItem);
                var jobProfileIndexSettings = _configuration.GetSection("AzureSearchSettings").Get<JobProfileIndexSettings>();
                var jobProfileIndexName = jobProfileIndexSettings.JobProfileSearchIndexName ?? string.Empty;

                SearchIndexClient indexClient = CreateSearchIndexClient(jobProfileIndexSettings.SearchServiceEndPoint ?? string.Empty, jobProfileIndexSettings.SearchServiceAdminAPIKey ?? string.Empty);
                if (await indexClient.GetIndexesAsync().AnyAsync(index => index.Name == jobProfileIndexName))
                {
                    SearchClient searchClient = indexClient.GetSearchClient(jobProfileIndexName);
                    if (actionType == ActionTypes.Published)
                    {

                        if (await indexClient.GetIndexesAsync().AnyAsync(index => index.Name == jobProfileIndexName))
                        {
                            var documentUploadResponse = UploadDocument(searchClient, jobProfileIndexDocument);
                            if (documentUploadResponse.IsCompletedSuccessfully) _logger.Log(LogLevel.Information, $"{jobProfileIndexDocument.IdentityField} successfully uploaded to Azure Search Index");
                        }

                    }
                    else if (actionType == ActionTypes.Deleted)
                    {
                        var documentUploadResponse = DeleteDocument(searchClient, jobProfileIndexDocument);
                        if (documentUploadResponse.IsCompletedSuccessfully) _logger.Log(LogLevel.Information, $"{jobProfileIndexDocument.IdentityField} successfully deleted document from Azure Search Index");
                    }
                }
                else                 // The creation of the index can be done at application start up
                {
                    if (CreateIndexAsync(jobProfileIndexName, indexClient).IsCompletedSuccessfully)
                    {
                        SearchClient searchClient = indexClient.GetSearchClient(jobProfileIndexName);
                        var documentUploadResponse = UploadDocument(searchClient, jobProfileIndexDocument);
                        if (documentUploadResponse.IsCompletedSuccessfully) _logger.Log(LogLevel.Information, $"{jobProfileIndexName} index successfully created and document{jobProfileIndexDocument.IdentityField} successfully uploaded to Azure Search Index");
                    }

                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to export data for item with ContentItemId = {context.ContentItem.ContentItemId}");
                throw;
            }
        }

        private static SearchIndexClient CreateSearchIndexClient(string searchServiceEndPoint, string adminApiKey)
        {
            SearchIndexClient indexClient = new SearchIndexClient(new Uri(searchServiceEndPoint), new AzureKeyCredential(adminApiKey));
            return indexClient;
        }

        private static Task<Response<SearchIndex>> CreateIndexAsync(string indexName, SearchIndexClient indexClient)
        {
            FieldBuilder builder = new FieldBuilder();
            var definition = new SearchIndex(indexName, builder.Build(typeof(JobProfileIndex)));
            return indexClient.CreateIndexAsync(definition);
        }

        private static Task<Response<IndexDocumentsResult>> UploadDocument(SearchClient searchClient, JobProfileIndex jobProfileIndexDocument)
        {
            IndexDocumentsBatch<JobProfileIndex> jobProfileIndexBatch = IndexDocumentsBatch.Create(IndexDocumentsAction.MergeOrUpload(jobProfileIndexDocument));
            IndexDocumentsOptions options = new IndexDocumentsOptions { ThrowOnAnyError = true };
            return searchClient.IndexDocumentsAsync(jobProfileIndexBatch, options);
        }

        private static Task<Response<IndexDocumentsResult>> DeleteDocument(SearchClient searchClient, JobProfileIndex jobProfileIndexDocument)
        {
            IndexDocumentsBatch<JobProfileIndex> jobProfileIndexBatch = IndexDocumentsBatch.Delete<JobProfileIndex>(new List<JobProfileIndex> { jobProfileIndexDocument });
            IndexDocumentsOptions options = new IndexDocumentsOptions { ThrowOnAnyError = true };
            return searchClient.IndexDocumentsAsync(jobProfileIndexBatch, options);
        }

    }
}
