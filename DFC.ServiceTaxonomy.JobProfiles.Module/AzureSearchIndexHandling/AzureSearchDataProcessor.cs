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
                    await CreateIndexAsync(jobProfileIndexName, indexClient);

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

        private static bool IsSuccessful(Response<IndexDocumentsResult> response) =>
            response.Value.Results.All(r => r.Succeeded);

        private static Task<Response<SearchIndex>> CreateIndexAsync(string indexName, SearchIndexClient indexClient)
        {
            FieldBuilder builder = new FieldBuilder();
            var definition = new SearchIndex(indexName, builder.Build(typeof(JobProfileIndex)));
            definition.Suggesters.Add(new SearchSuggester("sg", new[] {
                nameof(JobProfileIndex.Title),
                nameof(JobProfileIndex.AlternativeTitle)
            }));
            definition.ScoringProfiles.Add(new ScoringProfile("jp")
            {
                TextWeights = new TextWeights(
                new Dictionary<string, double>{
                  { nameof(JobProfileIndex.TitleAsKeyword), 100},
                  { nameof(JobProfileIndex.AltTitleAsKeywords), 100 },
                  { nameof(JobProfileIndex.Title), 7 },
                  { nameof(JobProfileIndex.AlternativeTitle), 6 },
                  { nameof(JobProfileIndex.Overview), 5 },
                  { nameof(JobProfileIndex.JobProfileCategories), 4 },
                  { nameof(JobProfileIndex.JobProfileSpecialism), 3 },
                  { nameof(JobProfileIndex.HiddenAlternativeTitle), 3 }
                })
            });
            return indexClient.CreateIndexAsync(definition);
        }

        private static Task<Response<IndexDocumentsResult>> UploadDocumentAsync(SearchClient searchClient, JobProfileIndex jobProfileIndexDocument)
        {
            IndexDocumentsBatch<JobProfileIndex> jobProfileIndexBatch = IndexDocumentsBatch.Create(IndexDocumentsAction.MergeOrUpload(jobProfileIndexDocument));
            IndexDocumentsOptions options = new IndexDocumentsOptions { ThrowOnAnyError = true };
            return searchClient.IndexDocumentsAsync(jobProfileIndexBatch, options);
        }

        private static Task<Response<IndexDocumentsResult>> DeleteDocumentAsync(SearchClient searchClient, JobProfileIndex jobProfileIndexDocument)
        {
            IndexDocumentsBatch<JobProfileIndex> jobProfileIndexBatch = IndexDocumentsBatch.Delete<JobProfileIndex>(new List<JobProfileIndex> { jobProfileIndexDocument });
            IndexDocumentsOptions options = new IndexDocumentsOptions { ThrowOnAnyError = true };
            return searchClient.IndexDocumentsAsync(jobProfileIndexBatch, options);
        }
    }
}
