using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Factories;
using GetJobProfiles.Models.Recipe.Fields;
using GetJobProfiles.Models.Recipe.Fields.Factories;
using GetJobProfiles.Models.Recipe.Parts;
using MoreLinq;
using OrchardCore.Entities;

namespace GetJobProfiles
{
    public class JobProfileConverter
    {
        public IEnumerable<JobProfileContentItem> JobProfiles { get; private set; } = Enumerable.Empty<JobProfileContentItem>();
        public readonly ContentPickerFactory Registrations = new ContentPickerFactory();
        public readonly ContentPickerFactory Restrictions = new ContentPickerFactory();
        public readonly ContentPickerFactory OtherRequirements = new ContentPickerFactory();
        public readonly ContentPickerFactory WorkingEnvironments = new ContentPickerFactory();
        public readonly ContentPickerFactory WorkingLocations = new ContentPickerFactory();
        public readonly ContentPickerFactory WorkingUniforms = new ContentPickerFactory();
        //todo: convert to factory?
        public readonly ConcurrentDictionary<string, (string id, string text)> DayToDayTasks = new ConcurrentDictionary<string, (string id, string text)>();

        public readonly ApprenticeshipRouteFactory ApprenticeshipRoutes = new ApprenticeshipRouteFactory();
        public readonly CollegeRouteFactory CollegeRoutes = new CollegeRouteFactory();
        public readonly UniversityRouteFactory UniversityRoutes = new UniversityRouteFactory();

        public string Timestamp { get; set; }

        private readonly RestHttpClient.RestHttpClient _client;
        private readonly Dictionary<string, string> _socCodeDictionary;
        private readonly DefaultIdGenerator _idGenerator;

        public List<string> DayToDayTaskExclusions = new List<string>
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
            //todo: use spreadsheet for titles

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

                    HtbRegistrations = Registrations.CreateContentPicker(jobProfile.HowToBecome.MoreInformation.Registrations),
                    WitDigitalSkillsLevel = new HtmlField(jobProfile.WhatItTakes.DigitalSkillsLevel),
                    WitRestrictions = Restrictions.CreateContentPicker(jobProfile.WhatItTakes.RestrictionsAndRequirements.RelatedRestrictions),
                    WitOtherRequirements = OtherRequirements.CreateContentPicker(jobProfile.WhatItTakes.RestrictionsAndRequirements.OtherRequirements),
                    SOCCode = new ContentPicker { ContentItemIds = new List<string> { _socCodeDictionary[jobProfile.Soc] } },
                    SalaryStarter = new TextField(jobProfile.SalaryStarter),
                    SalaryExperienced = new TextField(jobProfile.SalaryExperienced),
                    MinimumHours = new NumericField(jobProfile.MinimumHours),
                    MaximumHours = new NumericField(jobProfile.MaximumHours),
                    WorkingHoursDetails = new TextField(jobProfile.WorkingHoursDetails),
                    WorkingPattern = new TextField(jobProfile.WorkingPattern),
                    WorkingPatternDetails = new TextField(jobProfile.WorkingPatternDetails),
                    CareerPathAndProgression = new HtmlField(jobProfile.CareerPathAndProgression.CareerPathAndProgression),
                    WydWorkingEnvironment = WorkingEnvironments.CreateContentPicker(jobProfile.WhatYouWillDo?.WorkingEnvironment?.Environment),
                    WydWorkingLocation = WorkingLocations.CreateContentPicker(jobProfile.WhatYouWillDo?.WorkingEnvironment?.Location),
                    WydWorkingUniform = WorkingUniforms.CreateContentPicker(jobProfile.WhatYouWillDo?.WorkingEnvironment?.Uniform),
                },
                BagPart = new BagPart()
            };

            if (!jobProfile.HowToBecome.EntryRoutes.Apprenticeship.IsEmpty())
            {
                //todo: don't think academic entry routes require a title. hardcode display text?
                contentItem.BagPart.ContentItems.Add(ApprenticeshipRoutes.CreateApprenticeshipRoute(jobProfile.HowToBecome.EntryRoutes.Apprenticeship, null, Timestamp));
            }

            if (!jobProfile.HowToBecome.EntryRoutes.College.IsEmpty())
            {
                contentItem.BagPart.ContentItems.Add(CollegeRoutes.CreateCollegeRoute(jobProfile.HowToBecome.EntryRoutes.College, null, Timestamp));
            }

            if (!jobProfile.HowToBecome.EntryRoutes.University.IsEmpty())
            {
                contentItem.BagPart.ContentItems.Add(UniversityRoutes.CreateUniversityRoute(jobProfile.HowToBecome.EntryRoutes.University, null, Timestamp));
            }

            //todo: helper?
            if (jobProfile.HowToBecome.EntryRoutes.Work.Any())
            {
                contentItem.BagPart.ContentItems.Add(new WorkRouteContentItem(Timestamp, jobProfile.HowToBecome.EntryRoutes.Work));
            }

            if (jobProfile.HowToBecome.EntryRoutes.Volunteering.Any())
            {
                contentItem.BagPart.ContentItems.Add(new VolunteeringRouteContentItem(Timestamp, jobProfile.HowToBecome.EntryRoutes.Volunteering));
            }

            if (jobProfile.HowToBecome.EntryRoutes.DirectApplication.Any())
            {
                contentItem.BagPart.ContentItems.Add(new DirectRouteContentItem(Timestamp, jobProfile.HowToBecome.EntryRoutes.DirectApplication));
            }

            if (jobProfile.HowToBecome.EntryRoutes.OtherRoutes.Any())
            {
                contentItem.BagPart.ContentItems.Add(new OtherRouteContentItem(Timestamp, jobProfile.HowToBecome.EntryRoutes.OtherRoutes));
            }


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
