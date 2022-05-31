using System;
using System.Linq;
using System.Threading.Tasks;

using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.AzureSearch;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using OrchardCore.ContentManagement.Handlers;

using static DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch.SearchClientHelper;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch
{
    public class AzureSearchDataProcessor : IAzureSearchDataProcessor
    {
        private readonly ILogger<AzureSearchDataProcessor> _logger;
        private readonly IMessageConverter<JobProfileIndex> _jobProfileIndexConverter;
        private readonly JobProfileIndexSettings _jobProfileIndexSettings;

        public AzureSearchDataProcessor(ILogger<AzureSearchDataProcessor> logger, IMessageConverter<JobProfileIndex> jobProfileIndexConverter, IConfiguration configuration)
        {
            _logger = logger;
            _jobProfileIndexConverter = jobProfileIndexConverter;
            _jobProfileIndexSettings = configuration.GetSection("AzureSearchSettings").Get<JobProfileIndexSettings>()
                ?? throw new ArgumentNullException(nameof(configuration), $"{nameof(JobProfileIndexSettings)} configuration is invalid");

        }

        public async Task ProcessContentContext(ContentContextBase context, string actionType)
        {
            try
            {
                var jobProfileIndexDocument = await _jobProfileIndexConverter.ConvertFromAsync(context.ContentItem);
                var jobProfileIndexName = _jobProfileIndexSettings.JobProfileSearchIndexName ?? string.Empty;
                SearchIndexClient indexClient = CreateSearchIndexClient(_jobProfileIndexSettings);

                if (await indexClient.GetIndexesAsync().AnyAsync(index => index.Name == jobProfileIndexName))
                {
                    SearchClient searchClient = indexClient.GetSearchClient(jobProfileIndexName);
                    if (actionType.Equals(ActionTypes.Published))
                    {
                        var documentUploadResponse = await UploadDocumentAsync(searchClient, jobProfileIndexDocument);
                        if (IsSuccessful(documentUploadResponse))
                        {
                            _logger.Log(LogLevel.Information, $"{jobProfileIndexDocument.IdentityField} successfully uploaded to Azure Search Index");
                        }
                    }
                    else if (actionType.Equals(ActionTypes.Deleted))
                    {
                        var documentDeleteResponse = await DeleteDocumentAsync(searchClient, jobProfileIndexDocument);
                        if (IsSuccessful(documentDeleteResponse))
                        {
                            _logger.Log(LogLevel.Information, $"{jobProfileIndexDocument.IdentityField} successfully deleted document from Azure Search Index");
                        }
                    }
                }
                else                 // The creation of the index can be done at application start up
                {
                    SearchIndex definition = GetSearchIndexDefinition(jobProfileIndexName);
                    await indexClient.CreateIndexAsync(definition);

                    SearchClient searchClient = indexClient.GetSearchClient(jobProfileIndexName);
                    var documentUploadResponse = await UploadDocumentAsync(searchClient, jobProfileIndexDocument);
                    if (IsSuccessful(documentUploadResponse))
                    {
                        _logger.Log(LogLevel.Information, $"{jobProfileIndexName} index successfully created and document - {jobProfileIndexDocument.IdentityField} successfully uploaded to Azure Search Index");
                    }
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to export data for item with ContentItemId = {context.ContentItem.ContentItemId}");
                throw;
            }
        }

        private static SearchIndexClient CreateSearchIndexClient(JobProfileIndexSettings jobProfileIndexSettings)
        {
            var searchServiceEndPoint = jobProfileIndexSettings.SearchServiceEndPoint ?? string.Empty;
            var adminApiKey = jobProfileIndexSettings.SearchServiceAdminAPIKey ?? string.Empty;

            SearchIndexClient indexClient = new SearchIndexClient(new Uri(searchServiceEndPoint), new AzureKeyCredential(adminApiKey));
            return indexClient;
        }
    }
}
