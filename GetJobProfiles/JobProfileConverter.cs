using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe;
using MoreLinq;

namespace GetJobProfiles
{
    public class JobProfileConverter
    {
        public IEnumerable<JobProfileContentItem> JobProfiles { get; private set; } = Enumerable.Empty<JobProfileContentItem>();

        public string Timestamp { get; set; }

        private readonly RestHttpClient.RestHttpClient _client;

        public JobProfileConverter(RestHttpClient.RestHttpClient client)
        {
            _client = client;
            Timestamp = $"{DateTime.UtcNow:O}Z";
        }

        public async Task Go(int skip = 0, int take = 0, int napTimeMs = 5000)
        {
            var summaries = (await _client.Get<JobProfileSummary[]>("summary"))
                .Where(s => s.Title != null
                            && s.Title != "Api Test Profile"
                            && s.Title != "Jas test"
                            && s.Title != "Ismail Test Profile"
                            && s.Title != "Auditor Hari Test"
                            && s.Title != "Blacksmith LJ IC Test"
                            && s.Title != "Electrical engineering technician -test"
                            && s.Title != "GP - Karl"
                            && s.Title != "mktest"
                            && s.Title != "mktest1"
                            && s.Title != "Test 2"
                            && s.Title != "Test jP Hari"
                            && s.Title != "Vehicle body repairer - Test"
                            && s.Title != "Wedding planner Testing "
                            && s.Title != "Welly designer Amended"
                            && s.Title != "Zookeeper_ilyas1"
                            && s.Title != "This is my patched breadcrumb title"
                            && s.Title != "Technical brewer - GSR3");

            if (skip > 0)
                summaries = summaries.Skip(skip);
            if (take > 0)
                summaries = summaries.Take(take);

            //IEnumerable<JobProfileContentItem> contentItems = Enumerable.Empty<JobProfileContentItem>();
            var summaryBatches = summaries.Batch(10);
            foreach (var batch in summaryBatches)
            {
                var contentItemBatch = await GetAndConvert(_client, batch);
                JobProfiles = JobProfiles.Concat(contentItemBatch);
                // rate-limited
                if (napTimeMs == 0)
                    continue;

                Console.WriteLine("Taking a nap");
                Thread.Sleep(napTimeMs);
            }
        }

        private async Task<IEnumerable<JobProfileContentItem>> GetAndConvert(RestHttpClient.RestHttpClient client, IEnumerable<JobProfileSummary> jobProfileSummaries)
        {
            var getAndConvertTasks = jobProfileSummaries.Select(s => GetAndConvert(client, s));
            return await Task.WhenAll(getAndConvertTasks);
        }

        private async Task<JobProfileContentItem> GetAndConvert(RestHttpClient.RestHttpClient client, JobProfileSummary summary)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($">>> Fetching {summary.Title} job profile");
            var jobProfile = await client.Get<JobProfile>(new Uri(summary.Url, UriKind.Absolute));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"<<< Fetched {summary.Title} job profile");

            return ConvertJobProfile(jobProfile);
        }

        private JobProfileContentItem ConvertJobProfile(JobProfile jobProfile)
        {
            var contentItem = new JobProfileContentItem
            {
                //DisplayText vs Title
                ContentItemId = "[js:uuid()]",
                ContentItemVersionId = "[js:uuid()]",
                ContentType = "JobProfile",
                DisplayText = jobProfile.Title,
                Latest = true,
                Published = true,
                ModifiedUtc = Timestamp,
                PublishedUtc = Timestamp,
                CreatedUtc = Timestamp,
                Owner = "[js: parameters('AdminUsername')]",
                Author = "[js: parameters('AdminUsername')]",
                TitlePart = new TitlePart
                {
                    Title = jobProfile.Title
                },
                HtbBodies = new HtmlField(jobProfile.HowToBecome.MoreInformation.ProfessionalAndIndustryBodies),
                HtbCareerTips = new HtmlField(jobProfile.HowToBecome.MoreInformation.CareerTips),
                //todo:
                //HtbOtherRequirements = new HtmlField(jobProfile.HowToBecome.MoreInformation.
                HtbFurtherInformation = new HtmlField(jobProfile.HowToBecome.MoreInformation.FurtherInformation),
                //todo:
                //HtbTitleOptions = jobProfile.
                //todo: dic of contentid to found content: convert to class and have content as props
                HtbRegistrations = new ContentPicker()
            };

            return contentItem;
        }
    }
}
