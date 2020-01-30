using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe;
using MoreLinq;
using OrchardCore.Entities;

namespace GetJobProfiles
{
    public class JobProfileConverter
    {
        public IEnumerable<JobProfileContentItem> JobProfiles { get; private set; } = Enumerable.Empty<JobProfileContentItem>();
        public readonly ConcurrentDictionary<string,(string id,string text)> Registrations = new ConcurrentDictionary<string, (string id, string text)>();
        public readonly ConcurrentDictionary<string,(string id,string text)> Restrictions = new ConcurrentDictionary<string, (string id, string text)>();
        public readonly ConcurrentDictionary<string,(string id,string text)> OtherRequirements = new ConcurrentDictionary<string, (string id, string text)>();
        public readonly ConcurrentDictionary<string, (string id, string text)> DayToDayTasks = new ConcurrentDictionary<string, (string id, string text)>();

        public string Timestamp { get; set; }

        private readonly RestHttpClient.RestHttpClient _client;
        private readonly Dictionary<string, string> _socCodeDictionary;
        private readonly DefaultIdGenerator _idGenerator;

        public List<string> DayToDayTaskExclusions = new List<string>()
        {
            "https://dev.api.nationalcareersservice.org.uk/job-profiles/alexander-technique-teacher"
        };        

        public JobProfileConverter(RestHttpClient.RestHttpClient client, Dictionary<string, string> socCodeDictionary, string timestamp)
        {
            _client = client;
            _socCodeDictionary = socCodeDictionary;
            _idGenerator = new DefaultIdGenerator();
            Timestamp = timestamp;
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

                ColorConsole.WriteLine("Taking a nap");
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
            ColorConsole.WriteLine($">>> Fetching {summary.Title} job profile", ConsoleColor.DarkYellow);
            Console.WriteLine(await client.Get(new Uri(summary.Url, UriKind.Absolute)));
            var jobProfile = await client.Get<JobProfile>(new Uri(summary.Url, UriKind.Absolute));
            ColorConsole.WriteLine($"<<< Fetched {summary.Title} job profile", ConsoleColor.Yellow);

            return ConvertJobProfile(jobProfile);
        }

        private JobProfileContentItem ConvertJobProfile(JobProfile jobProfile)
        {
            //todo: might need access to internal api's to fetch these sort of things: title doesn't come through job profile api
            foreach (var registration in jobProfile.HowToBecome.MoreInformation.Registrations ?? Enumerable.Empty<string>())
            {
                // for now add full as title. once we have the full list can plug in current titles
                if (!Registrations.TryAdd(registration, (_idGenerator.GenerateUniqueId(), registration)))
                {
                    ColorConsole.WriteLine($"Registration '{registration}' already saved", ConsoleColor.Magenta);
                }
            }

            foreach (var restriction in jobProfile.WhatItTakes.RestrictionsAndRequirements.RelatedRestrictions ?? Enumerable.Empty<string>())
            {
                Console.WriteLine(restriction);

                // for now add full as title. once we have the full list can plug in current titles
                if (!Restrictions.TryAdd(restriction, (_idGenerator.GenerateUniqueId(), restriction)))
                {
                    ColorConsole.WriteLine($"Restriction '{restriction}' already saved", ConsoleColor.Magenta);
                }
            }

            foreach (var otherRequirement in jobProfile.WhatItTakes.RestrictionsAndRequirements.OtherRequirements ?? Enumerable.Empty<string>())
            {
                Console.WriteLine(otherRequirement);

                // for now add full as title. once we have the full list can plug in current titles
                if (!OtherRequirements.TryAdd(otherRequirement, (_idGenerator.GenerateUniqueId(), otherRequirement)))
                {
                    ColorConsole.WriteLine($"OtherRequirement '{otherRequirement}' already saved", ConsoleColor.Magenta);
                }
            }

            var contentItem = new JobProfileContentItem(jobProfile.Title, Timestamp)
            {
                //DisplayText vs Title
                // ContentItemId = "[js:uuid()]",
                // ContentItemVersionId = "[js:uuid()]",
                // ContentType = "JobProfile",
                // DisplayText = jobProfile.Title,
                // Latest = true,
                // Published = true,
                // ModifiedUtc = Timestamp,
                // PublishedUtc = Timestamp,
                // CreatedUtc = Timestamp,
                // Owner = "[js: parameters('AdminUsername')]",
                // Author = "[js: parameters('AdminUsername')]",
                // TitlePart = new TitlePart(jobProfile.Title),
                Description = new HtmlField(jobProfile.Overview),
                JobProfileWebsiteUrl = new TextField(jobProfile.Url),
                HtbBodies = new HtmlField(jobProfile.HowToBecome.MoreInformation.ProfessionalAndIndustryBodies),
                HtbCareerTips = new HtmlField(jobProfile.HowToBecome.MoreInformation.CareerTips),
                HtbFurtherInformation = new HtmlField(jobProfile.HowToBecome.MoreInformation.FurtherInformation),
                //todo:
                //HtbTitleOptions = jobProfile.
                //todo: dic of contentid to found content: convert to class and have content as props
                HtbRegistrations = new ContentPicker(Registrations, jobProfile.HowToBecome.MoreInformation.Registrations),
                WitDigitalSkillsLevel = new HtmlField(jobProfile.WhatItTakes.DigitalSkillsLevel),
                WitRestrictions = new ContentPicker(Restrictions, jobProfile.WhatItTakes.RestrictionsAndRequirements.RelatedRestrictions),
                WitOtherRequirements = new ContentPicker(OtherRequirements, jobProfile.WhatItTakes.RestrictionsAndRequirements.OtherRequirements),
                SOCCode = new ContentPicker { ContentItemIds = new List<string> { _socCodeDictionary[jobProfile.Soc] } }
            };

            if (!DayToDayTaskExclusions.Contains(jobProfile.Url))
            {
                var searchTerms = new[]
                {
                    "include:",
                    "be:",
                    "then:",
                    "like:",
                    "checking:"
                };

                if (jobProfile.WhatYouWillDo.WYDDayToDayTasks.All(x => !searchTerms.Any(t => x.Contains(t, StringComparison.OrdinalIgnoreCase))))
                    DayToDayTaskExclusions.Add(jobProfile.Url);

                var activities = jobProfile.WhatYouWillDo.WYDDayToDayTasks
                    .Where(x => searchTerms.Any(t => x.Contains(t, StringComparison.OrdinalIgnoreCase)))
                    .SelectMany(a => a.Substring(a.IndexOf(":") + 1).Split(";")).Select(x => x.Trim())
                    .ToList();

                foreach (var activity in activities)
                {
                    if (!DayToDayTasks.TryAdd(activity, (_idGenerator.GenerateUniqueId(), activity)))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"DayToDayTask '{activity}' already saved");
                    }
                }

                contentItem.DayToDayTasks = new ContentPicker(DayToDayTasks, activities);
            }

            return contentItem;
        }
    }
}
