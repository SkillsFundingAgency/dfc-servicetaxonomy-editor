using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.EqualityComparers;
using GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Factories;
using GetJobProfiles.Models.Recipe.ContentItems.EqualityComparers;
using GetJobProfiles.Models.Recipe.Fields;
using GetJobProfiles.Models.Recipe.Fields.Factories;
using GetJobProfiles.Models.Recipe.Parts;
using MoreLinq;
using OrchardCore.Entities;

namespace GetJobProfiles.Importers
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
        public readonly ImportedAcademicRouteEqualityComparer ImportedAcademicRouteEqualityComparer;
        public readonly ImportedTitleHtmlDescriptionEqualityComparer ImportedTitleHtmlDescriptionEqualityComparer;
        public readonly ContentPickerContentItemFactory<AcademicEntryRouteContentItem> ApprenticeshipRoute;
        public readonly ContentPickerContentItemFactory<AcademicEntryRouteContentItem> CollegeRoute;
        public readonly ContentPickerContentItemFactory<AcademicEntryRouteContentItem> UniversityRoute;
        public readonly ContentPickerContentItemFactory<TitleHtmlDescriptionContentItem> DirectRoute;
        public readonly ContentPickerContentItemFactory<TitleHtmlDescriptionContentItem> OtherRoute;
        public readonly ContentPickerContentItemFactory<TitleHtmlDescriptionContentItem> VolunteeringRoute;
        public readonly ContentPickerContentItemFactory<TitleHtmlDescriptionContentItem> WorkRoute;
        //todo: convert to factory?
        public readonly ConcurrentDictionary<string, (string id, string text)> DayToDayTasks = new ConcurrentDictionary<string, (string id, string text)>();

        public readonly ApprenticeshipRouteFactory ApprenticeshipRoutes = new ApprenticeshipRouteFactory();
        public readonly CollegeRouteFactory CollegeRoutes = new CollegeRouteFactory();
        public readonly UniversityRouteFactory UniversityRoutes = new UniversityRouteFactory();

        public string Timestamp { get; set; }

        private readonly RestHttpClient.RestHttpClient _client;
        private readonly SocCodeContentPickerFactory _socCodeContentPickerFactory;
        private readonly Dictionary<string, string> _oNetDictionary;
        //private readonly Dictionary<string, List<string>> _dysacSkillSocLookup;
        private readonly TitleOptionsTextFieldFactory _titleOptionsFactory;
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

        public JobProfileConverter(
            RestHttpClient.RestHttpClient client,
            Dictionary<string, string> socCodeDictionary,
            Dictionary<string, string> oNetDictionary,
            Dictionary<string, string> titleOptionsLookup,
            string timestamp)
        {
            _client = client;
            _socCodeContentPickerFactory = new SocCodeContentPickerFactory(socCodeDictionary);
            _oNetDictionary = oNetDictionary;
            _titleOptionsFactory = new TitleOptionsTextFieldFactory(titleOptionsLookup);
            _idGenerator = new DefaultIdGenerator();
            Timestamp = timestamp;

            ImportedAcademicRouteEqualityComparer = new ImportedAcademicRouteEqualityComparer();
            ImportedTitleHtmlDescriptionEqualityComparer = new ImportedTitleHtmlDescriptionEqualityComparer();
            ApprenticeshipRoute = new ContentPickerContentItemFactory<AcademicEntryRouteContentItem>(ImportedAcademicRouteEqualityComparer);
            CollegeRoute = new ContentPickerContentItemFactory<AcademicEntryRouteContentItem>(ImportedAcademicRouteEqualityComparer);
            UniversityRoute = new ContentPickerContentItemFactory<AcademicEntryRouteContentItem>(ImportedAcademicRouteEqualityComparer);
            DirectRoute = new ContentPickerContentItemFactory<TitleHtmlDescriptionContentItem>(ImportedTitleHtmlDescriptionEqualityComparer);
            OtherRoute = new ContentPickerContentItemFactory<TitleHtmlDescriptionContentItem>(ImportedTitleHtmlDescriptionEqualityComparer);
            VolunteeringRoute = new ContentPickerContentItemFactory<TitleHtmlDescriptionContentItem>(ImportedTitleHtmlDescriptionEqualityComparer);
            WorkRoute = new ContentPickerContentItemFactory<TitleHtmlDescriptionContentItem>(ImportedTitleHtmlDescriptionEqualityComparer);
        }

        public async Task Go(int skip = 0, int take = 0, int napTimeMs = 5000, string jobProfilesToImportCsv = null)
        {
            IEnumerable<JobProfileSummary> summaries;
            EscoJobProfileMapper exclusionSource = new EscoJobProfileMapper();

            if (!string.IsNullOrWhiteSpace(jobProfilesToImportCsv) && jobProfilesToImportCsv != "*")
            {
                string[] jobProfilesToImportUntrimmed = jobProfilesToImportCsv.Split(",");
                HashSet<string> jobProfilesToImport = new HashSet<string>(
                    jobProfilesToImportUntrimmed.Select(jp => jp.Trim().ToLowerInvariant()));

                summaries = (await _client.Get<JobProfileSummary[]>("summary"))
                    .Where(s => s.Title != null
                                && jobProfilesToImport.Contains(s.Title.ToLowerInvariant())
                                && !exclusionSource._exclusions.Contains(s.Title.ToLowerInvariant().Replace(" ", "-")));
            }
            else
            {
                summaries = (await _client.Get<JobProfileSummary[]>("summary"))
                    .Where(s => s.Title != null
                             && !exclusionSource._exclusions.Contains(s.Title.ToLowerInvariant().Replace(" ", "-")));
            }

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
            var uri = new Uri(jobProfile.Url);

            if (!string.IsNullOrWhiteSpace(jobProfile.WhatYouWillDo?.WorkingEnvironment?.Uniform) && (!jobProfile.WhatYouWillDo?.WorkingEnvironment?.Uniform?.StartsWith("You may need to wear ") ?? true))
            {
                throw new InvalidOperationException(
                    $"JobProfile {jobProfile.Title} does not conform to WydWorkingUniform prefix expectation");
            }

            string uniform = string.IsNullOrWhiteSpace(jobProfile.WhatYouWillDo?.WorkingEnvironment?.Uniform)
                ? string.Empty
                : jobProfile.WhatYouWillDo?.WorkingEnvironment?.Uniform?.Substring(21).Trim('.');

            if (!string.IsNullOrWhiteSpace(jobProfile.WhatYouWillDo?.WorkingEnvironment?.Location) && (!jobProfile.WhatYouWillDo?.WorkingEnvironment?.Location?.StartsWith("You could work ") ?? true))
            {
                throw new InvalidOperationException(
                    $"JobProfile {jobProfile.Title} does not conform to WydWorkingLocation prefix expectation");
            }

            string location = string.IsNullOrWhiteSpace(jobProfile.WhatYouWillDo?.WorkingEnvironment?.Location)
                ? string.Empty
                : jobProfile.WhatYouWillDo?.WorkingEnvironment?.Location?.Substring(15).Trim('.');

            if (!string.IsNullOrWhiteSpace(jobProfile.WhatYouWillDo?.WorkingEnvironment?.Environment) && (!jobProfile.WhatYouWillDo?.WorkingEnvironment?.Environment?.StartsWith("Your working environment may be ") ?? true))
            {
                throw new InvalidOperationException(
                    $"JobProfile {jobProfile.Title} does not conform to WydWorkingEnvironment prefix expectation");
            }

            string environment = string.IsNullOrWhiteSpace(jobProfile.WhatYouWillDo?.WorkingEnvironment?.Environment)
                ? string.Empty
                : jobProfile.WhatYouWillDo?.WorkingEnvironment?.Environment?.Substring(32).Trim('.');

            _oNetDictionary.TryGetValue(jobProfile.Title, out var oNetContentItemIds);

            oNetContentItemIds ??= _oNetDictionary[ONetConverter.UnknownJobProfile];

            string jobProfileWebsiteUrl = uri.Segments.Last();

            var contentItem = new JobProfileContentItem(jobProfile.Title, Timestamp)
            {
                PageLocationPart = new PageLocationPart
                {
                    UrlName = new TextField(jobProfileWebsiteUrl),
                    FullUrl = new TextField($"/job-profiles/{jobProfileWebsiteUrl}"),
                },
                EponymousPart = new JobProfilePart
                {
                    PageLocations = new TaxonomyField
                    {
                        TaxonomyContentItemId = "4eembshqzx66drajtdten34tc8",
                        TermContentItemIds = new [] { "4x352kh85x7894jr7yqbt0z34b" }
                    },
                    Description = new HtmlField(jobProfile.Overview),
                    TitleOptions = _titleOptionsFactory.Create(jobProfileWebsiteUrl),
                    SOCCode = _socCodeContentPickerFactory.Create(jobProfile.Soc),
                    ONetOccupationalCode = new ContentPicker { ContentItemIds = new List<string> { oNetContentItemIds } },
                    SalaryStarter = new TextField(jobProfile.SalaryStarter),
                    SalaryExperienced = new TextField(jobProfile.SalaryExperienced),
                    MinimumHours = new NumericField(jobProfile.MinimumHours),
                    MaximumHours = new NumericField(jobProfile.MaximumHours),
                    WorkingHoursDetails = new TextField(jobProfile.WorkingHoursDetails),
                    WorkingPattern = new TextField(jobProfile.WorkingPattern),
                    WorkingPatternDetails = new TextField(jobProfile.WorkingPatternDetails),
                    HtbBodies = new HtmlField(jobProfile.HowToBecome.MoreInformation.ProfessionalAndIndustryBodies),
                    HtbCareerTips = new HtmlField(jobProfile.HowToBecome.MoreInformation.CareerTips),
                    HtbFurtherInformation = new HtmlField(jobProfile.HowToBecome.MoreInformation.FurtherInformation),
                    HtbRegistrations = Registrations.CreateContentPicker(jobProfile.HowToBecome.MoreInformation.Registrations),
                    WitDigitalSkillsLevel = new HtmlField(jobProfile.WhatItTakes.DigitalSkillsLevel),
                    WitRestrictions = Restrictions.CreateContentPicker(jobProfile.WhatItTakes.RestrictionsAndRequirements.RelatedRestrictions),
                    WitOtherRequirements = OtherRequirements.CreateContentPicker(jobProfile.WhatItTakes.RestrictionsAndRequirements.OtherRequirements),
                    WydWorkingEnvironment = WorkingEnvironments.CreateContentPicker(environment),
                    WydWorkingLocation = WorkingLocations.CreateContentPicker(location),
                    WydWorkingUniform = WorkingUniforms.CreateContentPicker(uniform),
                    CareerPathAndProgression = new HtmlField(jobProfile.CareerPathAndProgression.CareerPathAndProgression)
                }
            };

            if (jobProfile.HowToBecome.EntryRoutes.Apprenticeship.IsEmpty())
            {
                contentItem.EponymousPart.ApprenticeshipRoute = new ContentPicker();
            }
            else
            {
                var apprenticeshipEntryRoute = ApprenticeshipRoutes.Create(contentItem.DisplayText,
                    jobProfile.HowToBecome.EntryRoutes.Apprenticeship, Timestamp);

                contentItem.EponymousPart.ApprenticeshipRoute = ApprenticeshipRoute.CreateContentPicker(apprenticeshipEntryRoute);
            }

            if (jobProfile.HowToBecome.EntryRoutes.College.IsEmpty())
            {
                contentItem.EponymousPart.CollegeRoute = new ContentPicker();
            }
            else
            {
                var collegeEntryRoute = CollegeRoutes.Create(contentItem.DisplayText,
                    jobProfile.HowToBecome.EntryRoutes.College, Timestamp);

                contentItem.EponymousPart.CollegeRoute = CollegeRoute.CreateContentPicker(collegeEntryRoute);
            }

            if (jobProfile.HowToBecome.EntryRoutes.University.IsEmpty())
            {
                contentItem.EponymousPart.UniversityRoute = new ContentPicker();
            }
            else
            {
                var universityEntryRoute = UniversityRoutes.Create(contentItem.DisplayText,
                    jobProfile.HowToBecome.EntryRoutes.University, Timestamp);

                contentItem.EponymousPart.UniversityRoute = UniversityRoute.CreateContentPicker(universityEntryRoute);
            }

            if (!jobProfile.HowToBecome.EntryRoutes.DirectApplication.Any())
            {
                contentItem.EponymousPart.DirectRoute = new ContentPicker();
            }
            else
            {
                var route = new DirectRouteContentItem(contentItem.DisplayText, Timestamp,
                    jobProfile.HowToBecome.EntryRoutes.DirectApplication);

                contentItem.EponymousPart.DirectRoute = DirectRoute.CreateContentPicker(route);
            }

            if (!jobProfile.HowToBecome.EntryRoutes.OtherRoutes.Any())
            {
                contentItem.EponymousPart.OtherRoute = new ContentPicker();
            }
            else
            {
                var route = new OtherRouteContentItem(contentItem.DisplayText, Timestamp,
                    jobProfile.HowToBecome.EntryRoutes.OtherRoutes);

                contentItem.EponymousPart.OtherRoute = OtherRoute.CreateContentPicker(route);
            }

            if (!jobProfile.HowToBecome.EntryRoutes.Volunteering.Any())
            {
                contentItem.EponymousPart.VolunteeringRoute = new ContentPicker();
            }
            else
            {
                var route = new VolunteeringRouteContentItem(contentItem.DisplayText, Timestamp,
                    jobProfile.HowToBecome.EntryRoutes.Volunteering);

                contentItem.EponymousPart.VolunteeringRoute = VolunteeringRoute.CreateContentPicker(route);
            }

            if (!jobProfile.HowToBecome.EntryRoutes.Work.Any())
            {
                contentItem.EponymousPart.WorkRoute = new ContentPicker();
            }
            else
            {
                var route = new WorkRouteContentItem(contentItem.DisplayText, Timestamp,
                    jobProfile.HowToBecome.EntryRoutes.Work);

                contentItem.EponymousPart.WorkRoute = WorkRoute.CreateContentPicker(route);
            }

            if (DayToDayTaskExclusions.Contains(jobProfile.Url))
            {
                contentItem.EponymousPart.DayToDayTasks = new ContentPicker();
            }
            else
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

        public void UpdateRouteItemsWithSharedNames()
        {
            UpdateRouteItemsWithSharedNames(ApprenticeshipRoute);
            UpdateRouteItemsWithSharedNames(CollegeRoute);
            UpdateRouteItemsWithSharedNames(UniversityRoute);
            UpdateRouteItemsWithSharedNames(DirectRoute);
            UpdateRouteItemsWithSharedNames(OtherRoute);
            UpdateRouteItemsWithSharedNames(WorkRoute);
            UpdateRouteItemsWithSharedNames(VolunteeringRoute);
        }

        // it's all a bit messy, c++'s specialization would be helpful, or we could introduce a new common base class/interface, perhaps IContentItemWithTitle
        private void UpdateRouteItemsWithSharedNames(ContentPickerContentItemFactory<AcademicEntryRouteContentItem> factory)
        {
            foreach (var itemToUser in factory.ItemToCompositeName)
            {
                itemToUser.Key.TitlePart.Title = itemToUser.Key.DisplayText = itemToUser.Value;
            }
        }

        private void UpdateRouteItemsWithSharedNames(ContentPickerContentItemFactory<TitleHtmlDescriptionContentItem> factory)
        {
            foreach (var itemToUser in factory.ItemToCompositeName)
            {
                itemToUser.Key.TitlePart.Title = itemToUser.Key.DisplayText = itemToUser.Value;
            }
        }
    }
}
