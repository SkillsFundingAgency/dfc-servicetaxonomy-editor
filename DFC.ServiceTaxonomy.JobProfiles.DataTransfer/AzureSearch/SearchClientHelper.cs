using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;

using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.AzureSearch;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch
{
    public static class SearchClientHelper
    {

        public static Task<Response<IndexDocumentsResult>> UploadDocumentAsync(SearchClient searchClient, JobProfileIndex jobProfileIndexDocument)
        {
            IndexDocumentsBatch<JobProfileIndex> jobProfileIndexBatch = IndexDocumentsBatch.Create(IndexDocumentsAction.MergeOrUpload(jobProfileIndexDocument));
            IndexDocumentsOptions options = new IndexDocumentsOptions { ThrowOnAnyError = true };
            return searchClient.IndexDocumentsAsync(jobProfileIndexBatch, options);
        }

        public static Task<Response<IndexDocumentsResult>> DeleteDocumentAsync(SearchClient searchClient, JobProfileIndex jobProfileIndexDocument)
        {
            IndexDocumentsBatch<JobProfileIndex> jobProfileIndexBatch = IndexDocumentsBatch.Delete(new List<JobProfileIndex> { jobProfileIndexDocument });
            IndexDocumentsOptions options = new IndexDocumentsOptions { ThrowOnAnyError = true };
            return searchClient.IndexDocumentsAsync(jobProfileIndexBatch, options);
        }

        public static SearchIndex GetSearchIndexDefinition(string indexName)
        {
            var builder = new FieldBuilder();
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
            return definition;
        }

        public static bool IsSuccessful(Response<IndexDocumentsResult> response) =>
            response.Value.Results.All(r => r.Succeeded);

    }
}
