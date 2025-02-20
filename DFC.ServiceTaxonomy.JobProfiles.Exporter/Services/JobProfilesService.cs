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

        public async Task<List<string>> GetAllJobProfilesUrls()
        {
            try
            {
                var jobProfiles = await _jobProfileRepository.LoadAll(new LoadAllRepoRequest());

                List<string> fullUrls = jobProfiles
                    .Where(jp => !string.IsNullOrEmpty(jp.PageLocation.FullUrl))
                    .Select(jp => jp.PageLocation.FullUrl)
                    .OrderBy(url => url)
                    .ToList();

                return fullUrls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error has occcured. {Message}", ex.Message);
                throw;
            }
        }

        public async Task<MemoryStream> GenerateCsvStreamAsync()
        {
            var jobProfilesUrls = await GetAllJobProfilesUrls();
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            // Dynamically determine column headers
            bool headerWritten = false;

            foreach (string url in jobProfilesUrls)
            {
                string fullUrl = $"https://nationalcareers.service.gov.uk/job-profiles{url}";
                var dynamicUrls = new List<string>();

                dynamicUrls.AddRange(await GetUrls(_howToBecomeRepository, url));
                dynamicUrls.AddRange(await GetUrls(_careerPathRepository, url));
                dynamicUrls.AddRange(await GetUrls(_whatItTakesRepository, url));
                dynamicUrls.AddRange(await GetUrls(_whatYouWillDoRepository, url));

                var record = new Dictionary<string, string>
                {
                    { "Job Profile Name", url },
                    { "Full URL", fullUrl }
                };

                for (int i = 0; i < dynamicUrls.Count; i++)
                {
                    record[$"URL {i + 1}"] = dynamicUrls[i];
                }

                if (!headerWritten)
                {
                    foreach (string column in record.Keys)
                    {
                        csv.WriteField(column);
                    }
                    await csv.NextRecordAsync();
                    headerWritten = true;
                }

                foreach (string value in record.Values)
                {
                    csv.WriteField(value);
                }
                await csv.NextRecordAsync();
            }

            await writer.FlushAsync();
            memoryStream.Position = 0;
            return memoryStream;
        }

        private static async Task<List<string>> GetUrls<T>(IJobProfileDataRepository<T> repository, string jobProfileName) where T : class
        {
            var data = await repository.GetItemByKey(new GetItemByKeyRepoRequest { Key = jobProfileName });
            if (data == null)
            {
                return new List<string>();
            }

            string json = JsonSerializer.Serialize(data);
            return ExtractHtmlUrls(json);
        }

        private static List<string> ExtractHtmlUrls(string jsonString)
        {
            var urls = new List<string>();
            var jsonDoc = JsonDocument.Parse(jsonString);
            ExtractUrls(jsonDoc.RootElement, urls);
            return urls;
        }

        private static void ExtractUrls(JsonElement element, List<string> urls)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (property.Name == "html" && property.Value.ValueKind == JsonValueKind.String)
                    {
                        string htmlContent = property.Value.GetString();
                        ExtractUrlsFromHtml(htmlContent, urls);
                    }
                    else
                    {
                        ExtractUrls(property.Value, urls);
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    ExtractUrls(item, urls);
                }
            }
        }
        private static void ExtractUrlsFromHtml(string html, List<string> urls)
        {
            if (string.IsNullOrWhiteSpace(html)) return;

            const string pattern = @"https?:\/\/[^\s""<>]+";
            foreach (Match match in Regex.Matches(html, pattern))
            {
                urls.Add(match.Value);
            }
        }
    }
}

