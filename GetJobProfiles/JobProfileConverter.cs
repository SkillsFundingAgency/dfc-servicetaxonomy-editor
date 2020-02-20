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
        public readonly ConcurrentDictionary<string, (string id, string text)> WorkingEnvironments = new ConcurrentDictionary<string, (string id, string text)>();
        public readonly ConcurrentDictionary<string, (string id, string text)> WorkingLocations = new ConcurrentDictionary<string, (string id, string text)>();
        public readonly ConcurrentDictionary<string, (string id, string text)> WorkingUniforms = new ConcurrentDictionary<string, (string id, string text)>();
        
        public string Timestamp { get; set; }

        private readonly RestHttpClient.RestHttpClient _client;
        private readonly Dictionary<string, string> _socCodeDictionary;
        private readonly DefaultIdGenerator _idGenerator;

        public List<string> DayToDayTaskExclusions = new List<string>()
        {
            "https://pp.api.nationalcareers.service.gov.uk/job-profiles/alexander-technique-teacher",
            "https://pp.api.nationalcareers.service.gov.uk/job-profiles/diver",
            "https://pp.api.nationalcareers.service.gov.uk/job-profiles/coroner",
            "https://pp.api.nationalcareers.service.gov.uk/job-profiles/demolition-operative",
            "https://pp.api.nationalcareers.service.gov.uk/job-profiles/football-coach",
            "https://pp.api.nationalcareers.service.gov.uk/job-profiles/head-of-it-(it-director)",
            "https://pp.api.nationalcareers.service.gov.uk/job-profiles/image-consultant",
            "https://pp.api.nationalcareers.service.gov.uk/job-profiles/personal-assistant",
            "https://pp.api.nationalcareers.service.gov.uk/job-profiles/plumber",
            "https://pp.api.nationalcareers.service.gov.uk/job-profiles/raf-airman-or-airwoman",
            "https://pp.api.nationalcareers.service.gov.uk/job-profiles/commercial-energy-assessor"
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
            var getTasks = jobProfileSummaries.Select(s => Get(client, s));
            var jobProfiles = await Task.WhenAll(getTasks);
            //todo: log we've excluded a jp, not that we can log the title!
            return jobProfiles.Where(jp => jp.Title != null)
                .Select(ConvertJobProfile);
        }

        private async Task<JobProfile> Get(RestHttpClient.RestHttpClient client, JobProfileSummary summary)
        {
            ColorConsole.WriteLine($">>> Fetching {summary.Title} job profile", ConsoleColor.DarkYellow);
            var jobProfile = await client.Get<JobProfile>(new Uri(summary.Url, UriKind.Absolute));
            ColorConsole.WriteLine($"<<< Fetched {summary.Title} job profile", ConsoleColor.Yellow);

            return jobProfile;
        }

        private JobProfileContentItem ConvertJobProfile(JobProfile jobProfile)
        {
            //todo: might need access to internal api's to fetch these sort of things: title doesn't come through job profile api
            foreach (string registration in jobProfile.HowToBecome.MoreInformation.Registrations ?? Enumerable.Empty<string>())
            {
                // for now add full as title. once we have the full list can plug in current titles
                if (!Registrations.TryAdd(registration, (_idGenerator.GenerateUniqueId(), registration)))
                {
                    ColorConsole.WriteLine($"Registration '{registration}' already saved", ConsoleColor.Magenta);
                }
            }

            foreach (string restriction in jobProfile.WhatItTakes.RestrictionsAndRequirements.RelatedRestrictions ?? Enumerable.Empty<string>())
            {
                // for now add full as title. once we have the full list can plug in current titles
                if (!Restrictions.TryAdd(restriction, (_idGenerator.GenerateUniqueId(), restriction)))
                {
                    ColorConsole.WriteLine($"Restriction '{restriction}' already saved", ConsoleColor.Magenta);
                }
            }

            foreach (string otherRequirement in jobProfile.WhatItTakes.RestrictionsAndRequirements.OtherRequirements ?? Enumerable.Empty<string>())
            {
                // for now add full as title. once we have the full list can plug in current titles
                if (!OtherRequirements.TryAdd(otherRequirement, (_idGenerator.GenerateUniqueId(), otherRequirement)))
                {
                    ColorConsole.WriteLine($"OtherRequirement '{otherRequirement}' already saved", ConsoleColor.Magenta);
                }
            }

            var workEnvironment = jobProfile.WhatYouWillDo?.WorkingEnvironment?.Environment;
            var workLocation = jobProfile.WhatYouWillDo?.WorkingEnvironment?.Location;
            var workUniform = jobProfile.WhatYouWillDo?.WorkingEnvironment?.Uniform;

            if (!string.IsNullOrWhiteSpace(workEnvironment) && !WorkingEnvironments.TryAdd(workEnvironment, (_idGenerator.GenerateUniqueId(), workEnvironment)))
            {
                ColorConsole.WriteLine($"WorkingEnvironment '{workEnvironment}' already saved", ConsoleColor.Magenta);
            }

            if (!string.IsNullOrWhiteSpace(workLocation) && !WorkingLocations.TryAdd(workLocation, (_idGenerator.GenerateUniqueId(), workLocation)))
            {
                ColorConsole.WriteLine($"WorkingLocation '{workLocation}' already saved", ConsoleColor.Magenta);
            }

            if (!string.IsNullOrWhiteSpace(workUniform) && !WorkingUniforms.TryAdd(workUniform, (_idGenerator.GenerateUniqueId(), workUniform)))
            {
                ColorConsole.WriteLine($"WorkingUniform '{workUniform}' already saved", ConsoleColor.Magenta);
            }

            var contentItem = new JobProfileContentItem(jobProfile.Title, Timestamp)
            {
                EponymousPart = new JobProfilePart
                {
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
                    SOCCode = new ContentPicker { ContentItemIds = new List<string> { _socCodeDictionary[jobProfile.Soc] } },
                    SalaryStarter = new TextField(jobProfile.SalaryStarter),
                    SalaryExperienced = new TextField(jobProfile.SalaryExperienced),
                    MinimumHours = new NumericField(jobProfile.MinimumHours),
                    MaximumHours = new NumericField(jobProfile.MaximumHours),
                    WorkingHoursDetails = new TextField(jobProfile.WorkingHoursDetails),
                    WorkingPattern = new TextField(jobProfile.WorkingPattern),
                    WorkingPatternDetails = new TextField(jobProfile.WorkingPatternDetails),
                    CareerPathAndProgression = new HtmlField(jobProfile.CareerPathAndProgression.CareerPathAndProgression),
                    WydWorkingEnvironment = new ContentPicker(WorkingEnvironments, workEnvironment),
                    WydWorkingLocation = new ContentPicker(WorkingLocations, workLocation),
                    WydWorkingUniform = new ContentPicker(WorkingUniforms, workUniform)
                }
            };

            if (!DayToDayTaskExclusions.Contains(jobProfile.Url))
            {
                var searchTerms = new[]
                {
                    "include:",
                    "be:",
                    "then:",
                    "like:",
                    "checking:",
                    "time:",
                    "involve:",
                    "you’ll:",
                    "you'll",
                    ":using",
                    "usually:",
                    "in:",
                    "to:",
                    "by:",
                    "with:",
                    "are:",
                    "might:",
                    "could:",
                    "types:",
                    "a:"
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
                        ColorConsole.WriteLine($"DayToDayTask '{activity}' already saved", ConsoleColor.Red);
                    }
                }

                contentItem.EponymousPart.DayToDayTasks = new ContentPicker(DayToDayTasks, activities);
            }

            return contentItem;
        }
    }
}
