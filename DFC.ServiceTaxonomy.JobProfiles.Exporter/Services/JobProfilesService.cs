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

            string[] sectionNames = { "How to Become", "What It Takes", "What You'll Do", "Career Path and Progression" };
            int[] maxUrlsPerSection = new int[sectionNames.Length];

            const int batchSize = 50;
            int totalProfiles = jobProfilesUrls.Count;

            var allProfilesData = new List<(string JobProfileName, string FullUrl, List<string>[] SectionUrls)>();

            // First Pass: Find maximum URLs per section
            for (int i = 0; i < totalProfiles; i += batchSize)
            {
                var batch = jobProfilesUrls.Skip(i).Take(batchSize).ToList();

                var batchResults = await Task.WhenAll(batch.Select(async jobProfileName =>
                {
                    var sectionUrls = await GetAllUrlsForJobProfile(jobProfileName);

                    lock (maxUrlsPerSection)
                    {
                        for (int j = 0; j < sectionUrls.Length; j++)
                        {
                            maxUrlsPerSection[j] = Math.Max(maxUrlsPerSection[j], sectionUrls[j].Count);
                        }
                    }

                    return (JobProfileName: jobProfileName, FullUrl: $"https://nationalcareers.service.gov.uk/job-profiles{jobProfileName}", SectionUrls: sectionUrls);
                }));

                allProfilesData.AddRange(batchResults);
            }

            await using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
            await using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteField("Job Profile Name");
                csv.WriteField("Full URL");

                for (int sectionIndex = 0; sectionIndex < sectionNames.Length; sectionIndex++)
                {
                    for (int j = 1; j <= maxUrlsPerSection[sectionIndex]; j++)
                    {
                        csv.WriteField($"{sectionNames[sectionIndex]}");
                    }
                }

                await csv.NextRecordAsync();

                // Second Pass: Write CSV data
                for (int i = 0; i < totalProfiles; i += batchSize)
                {
                    var batch = allProfilesData.Skip(i).Take(batchSize).ToList();

                    foreach (var profile in batch)
                    {
                        csv.WriteField(profile.JobProfileName);
                        csv.WriteField(profile.FullUrl);

                        for (int sectionIndex = 0; sectionIndex < profile.SectionUrls.Length; sectionIndex++)
                        {
                            var urls = profile.SectionUrls[sectionIndex];

                            for (int j = 0; j < maxUrlsPerSection[sectionIndex]; j++)
                            {
                                csv.WriteField(j < urls.Count ? urls[j] : "");
                            }
                        }

                        await csv.NextRecordAsync();
                    }
                }

                await writer.FlushAsync();
            }

            memoryStream.Position = 0;
            _logger.LogInformation("Successfully generated the CSV stream. {Count} job profiles processed", totalProfiles);

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
                    GetUrls(_careerPathRepository, url),
                    GetUrls(_whatItTakesRepository, url),
                    GetUrls(_whatYouWillDoRepository, url)
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
