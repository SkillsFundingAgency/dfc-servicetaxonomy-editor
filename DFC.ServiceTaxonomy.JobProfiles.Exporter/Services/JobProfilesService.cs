using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using DfE.NCS.Framework.Core.Repositories.Models;
using DfE.NCS.Web.ExploreCareers.Data.Models.Response.JobProfile;
using DfE.NCS.Web.ExploreCareers.Data.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.JobProfiles.Exporter.Services
{
    public class JobProfilesService : IJobProfilesService
    {
        private readonly IJobProfileRepository _jobProfileRepository;
        private readonly IJobProfileDataRepository<JobProfileHowToBecome> _howToBecomeRepository;
        private readonly IJobProfileDataRepository<JobProfileCareerPath> _careerPathRepository;
        private readonly IJobProfileDataRepository<JobProfileWhatYouWillDo> _whatYouWillDoRepository;
        private readonly IJobProfileDataRepository<JobProfileWhatItTakes> _whatItTakesRepository;
        private readonly ILogger<JobProfilesService> _logger;
        private static readonly Regex UrlRegex = new(@"https?:\/\/[^\s""<>]+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public JobProfilesService(
            IJobProfileRepository jobProfileRepository,
            IJobProfileDataRepository<JobProfileHowToBecome> howToBecomeRepository,
            IJobProfileDataRepository<JobProfileCareerPath> careerPathRepository,
            IJobProfileDataRepository<JobProfileWhatYouWillDo> whatYouWillDoRepository,
            IJobProfileDataRepository<JobProfileWhatItTakes> whatItTakesRepository,
            ILogger<JobProfilesService> logger)
        {
            _jobProfileRepository = jobProfileRepository;
            _howToBecomeRepository = howToBecomeRepository;
            _careerPathRepository = careerPathRepository;
            _whatYouWillDoRepository = whatYouWillDoRepository;
            _whatItTakesRepository = whatItTakesRepository;
            _logger = logger;
        }

        public async Task<MemoryStream> GenerateCsvStreamAsync()
        {
            _logger.LogInformation("Attempting to generate the CSV stream");

            var jobProfilesUrls = await GetAllJobProfilesUrls();

            var memoryStream = new MemoryStream();

            await using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
            await using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteField("Job Profile Name");
                csv.WriteField("Full URL");
                await csv.NextRecordAsync();

                const int batchSize = 50;
                int totalProfiles = jobProfilesUrls.Count;

                for (int i = 0; i < totalProfiles; i += batchSize)
                {
                    var batch = jobProfilesUrls.Skip(i).Take(batchSize).ToList();

                    var results = await Task.WhenAll(batch.Select(async url =>
                    {
                        string fullUrl = $"https://nationalcareers.service.gov.uk/job-profiles{url}";
                        var dynamicUrls = new List<string>();

                        try
                        {
                            var results = await GetAllUrlsForJobProfile(url);
                            dynamicUrls.AddRange(results.SelectMany(r => r));
                            _logger.LogInformation("Fetched {Count} URLs for job profile: {Url}", dynamicUrls.Count, url);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to retrieve URLs for job profile: {Url}", url);
                            dynamicUrls.Add("Error: Failed to retrieve URLs");
                        }

                        return new
                        {
                            JobProfileName = url,
                            FullUrl = fullUrl,
                            Urls = dynamicUrls
                        };
                    }));

                    foreach (var result in results)
                    {
                        csv.WriteField(result.JobProfileName);
                        csv.WriteField(result.FullUrl);

                        foreach (string url in result.Urls)
                        {
                            csv.WriteField(url);
                        }
                        await csv.NextRecordAsync();
                    }
                }
                await writer.FlushAsync();
            }
            memoryStream.Position = 0;

            _logger.LogInformation("Successfully generated the CSV stream. {Count} job profiles processed", jobProfilesUrls.Count);

            return memoryStream;
        }

        public async Task<List<string>> GetAllJobProfilesUrls()
        {
            try
            {
                _logger.LogInformation("Attempting to fetch all job profiles");

                var jobProfiles = await _jobProfileRepository.LoadAll(new LoadAllRepoRequest());

                List<string> fullUrls = jobProfiles
                    .Where(jp => !string.IsNullOrEmpty(jp.PageLocation.FullUrl))
                    .Select(jp => jp.PageLocation.FullUrl)
                    .OrderBy(url => url)
                    .ToList();

                _logger.LogInformation("Found {Count} job profiles", fullUrls.Count);
                return fullUrls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error has occcured. {Message}", ex.Message);
                throw;
            }
        }

        private async Task<List<string>[]> GetAllUrlsForJobProfile(string url)
        {
            try
            {
                var urlTasks = new[]
                {
                    GetUrls(_howToBecomeRepository, url),
                    GetUrls(_whatItTakesRepository, url),
                    GetUrls(_whatYouWillDoRepository, url),
                    GetUrls(_careerPathRepository, url)
                };

                return await Task.WhenAll(urlTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve URLs for job profile: {Url}", url);
                throw;
            }
        }

        private static async Task<List<string>> GetUrls<T>(IJobProfileDataRepository<T> repository, string jobProfileName) where T : class
        {
            var data = await repository.GetItemByKey(new GetItemByKeyRepoRequest { Key = jobProfileName });
            if (data == null)
            {
                return new List<string>();
            }

            string json = JsonSerializer.Serialize(data);
            using var jsonDoc = JsonDocument.Parse(json);
            return ExtractUrls(jsonDoc.RootElement);
        }

        private static List<string> ExtractUrls(JsonElement element)
        {
            var urls = new List<string>();

            #if NET9_0_OR_GREATER
            System.Threading.Lock lockObj = new object();
            #else
            var lockObj = new object();
            #endif

            ExtractUrlsInternal(element);
            return urls;

            void ExtractUrlsInternal(JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in element.EnumerateObject())
                    {
                        if (property.Name == "html" && property.Value.ValueKind == JsonValueKind.String)
                        {
                            lock (lockObj)
                            {
                                ExtractUrlsFromHtml(property.Value.GetString(), urls);
                            }
                        }
                        else
                        {
                            ExtractUrlsInternal(property.Value);
                        }
                    }
                }
                else if (element.ValueKind == JsonValueKind.Array)
                {
                    Parallel.ForEach(element.EnumerateArray(), ExtractUrlsInternal);
                }
            }
        }

        private static void ExtractUrlsFromHtml(string html, List<string> urls)
        {
            if (string.IsNullOrWhiteSpace(html)) return;

            foreach (var match in UrlRegex.EnumerateMatches(html))
            {
                string matchedUrl = html.Substring(match.Index, match.Length);
                urls.Add(matchedUrl);
            }
        }
    }
}
