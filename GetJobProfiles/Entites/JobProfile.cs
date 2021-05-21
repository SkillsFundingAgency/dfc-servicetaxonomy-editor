using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Containers;
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
using Microsoft.Extensions.Configuration;
using MoreLinq;
using NPOI.XSSF.UserModel;
using OrchardCore.Entities;


namespace GetJobProfiles.Entities
{
    public class JobProfile
    {
        public IEnumerable<JobProfileContentItem> jobProfileContentItems
        {
            get;
            private set;
        } = Enumerable.Empty<JobProfileContentItem>();

        // Create available related reference data lists
        // Overview Section
        // Title Options
        // Soc Code
        // Onet Occupational Code

        /// How To Become Section
        // University Route
        public readonly ContentPickerContentItemFactory<AcademicEntryRouteContentItem> UniversityRoute;
        public readonly UniversityRouteFactory UniversityRoutes = new UniversityRouteFactory();

        // College Route
        public readonly ContentPickerContentItemFactory<AcademicEntryRouteContentItem> CollegeRoute;
        public readonly CollegeRouteFactory CollegeRoutes = new CollegeRouteFactory();

        // Apprenticeship Route
        public readonly ContentPickerContentItemFactory<AcademicEntryRouteContentItem> ApprenticeshipRoute;
        public readonly ApprenticeshipRouteFactory ApprenticeshipRoutes = new ApprenticeshipRouteFactory();

        // Work Route
        public readonly ContentPickerContentItemFactory<TitleHtmlDescriptionContentItem> WorkRoute;

        // Volunteering Route
        public readonly ContentPickerContentItemFactory<TitleHtmlDescriptionContentItem> VolunteeringRoute;

        // Direct Route
        public readonly ContentPickerContentItemFactory<TitleHtmlDescriptionContentItem> DirectRoute;

        // Other Route
        public readonly ContentPickerContentItemFactory<TitleHtmlDescriptionContentItem> OtherRoute;

        // Registrations
        public readonly ContentPickerFactory Registrations = new ContentPickerFactory();

        /// What It Takes Section
        // Restrictions
        public readonly ContentPickerFactory Restrictions = new ContentPickerFactory();

        // Other Requirements
        public readonly ContentPickerFactory OtherRequirements = new ContentPickerFactory();

        // Digital Skills Level
        public readonly ContentPickerFactory DigitalSkillsLevels = new ContentPickerFactory();

        // What You Will Do Section
        // Working Environment
        public readonly ContentPickerFactory WorkingEnvironments = new ContentPickerFactory();

        // Working Location
        public readonly ContentPickerFactory WorkingLocations = new ContentPickerFactory();

        // Working Uniform
        public readonly ContentPickerFactory WorkingUniforms = new ContentPickerFactory();

        public readonly ImportedAcademicRouteEqualityComparer ImportedAcademicRouteEqualityComparer;
        public readonly ImportedTitleHtmlDescriptionEqualityComparer ImportedTitleHtmlDescriptionEqualityComparer;

        //todo: convert to factory?
        public readonly ConcurrentDictionary<string, (string id, string text)> DayToDayTasks = new ConcurrentDictionary<string, (string id, string text)>();


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


        private readonly SocCodeContentPickerFactory _socCodeContentPickerFactory;
        private readonly TitleOptionsTextFieldFactory _titleOptionsFactory;
        private readonly DefaultIdGenerator _idGenerator;
        private readonly DigitalSkillsLevelContentPickerFactory _digitalSkillsLevelContentPickerFactory;


        private RestHttpClient.RestHttpClient _jobProfileApiRestHttpClient;
        private Dictionary<string, JobProfileExcelWorkbookColumnsModel> jobProfileExcelWorkbookColumnsDataModel;

        // local ctor members
        private readonly SettingsModel _settings;
        private readonly ReferenceData _refData;

        public JobProfile(
            SettingsModel settings,
            ReferenceData refData)
        {
            _settings = settings;
            _refData = refData;

            //_idGenerator = new DefaultIdGenerator();

            // Get RestHttpClient to access the JobProfile API
            _jobProfileApiRestHttpClient = GetJobProfileApiRestHttpClient(settings);

            // Initialise factories
            jobProfileExcelWorkbookColumnsDataModel = new JobProfileExcelWorkbookDataModelBuilder().Import(refData.JobProfileExcelWorkbook);
            _socCodeContentPickerFactory = new SocCodeContentPickerFactory(refData.SocCodes.SocCodesDictionary);
            _titleOptionsFactory = new TitleOptionsTextFieldFactory(jobProfileExcelWorkbookColumnsDataModel);
            _digitalSkillsLevelContentPickerFactory = new DigitalSkillsLevelContentPickerFactory(jobProfileExcelWorkbookColumnsDataModel);

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

        /// <summary>
        /// Gets a list of JobProfileSummary items from the JobProfile API and then iterates through that list calling the JobProfil API to get the JobProfile details.
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="napTimeMs"></param>
        /// <param name="jobProfilesToImportCsv"></param>
        /// <returns>Task</returns>
        public JobProfile Build(
            int skip = 0,
            int take = 0,
            int napTimeMs = 5000,
            string jobProfilesToImportCsv = null)
        {
            var jobProfileSummaryBatches = GetApiJobProfiles(jobProfilesToImportCsv, skip, take, napTimeMs).GetAwaiter().GetResult();

            foreach (var jobProfileSummaryBatch in jobProfileSummaryBatches)
            {
                var jobProfileContentItemsBatch = GetAPIJobProfileDetailAndConvertToContentItems(_jobProfileApiRestHttpClient, jobProfileSummaryBatch);

                // Add this batch of JobProfileContentItems to the list
                jobProfileContentItems = jobProfileContentItems.Concat(jobProfileContentItemsBatch.GetAwaiter().GetResult());

                // rate-limited (APIM)
                if (napTimeMs == 0)
                    continue;

                ColorConsole.WriteLine("APIM rate limiting delay {napTimeMs} (milliseconds)");
                Task.Delay(napTimeMs);
            }

            return this;
        }

        private async Task<IEnumerable<JobProfileContentItem>> GetAPIJobProfileDetailAndConvertToContentItems(RestHttpClient.RestHttpClient client, IEnumerable<JobProfileSummaryApiModel> jobProfileSummaries)
        {
            var getJobProfileDetails = jobProfileSummaries.Select(s => GetAPIJobProfileDetail(client, s));
            var jobProfiles = await Task.WhenAll(getJobProfileDetails);

            //todo: log we've excluded a jp, not that we can log the title!
            return jobProfiles.Where(jp => jp.Title != null).Select(CreateJobProfileContentItem);
        }

        private async Task<JobProfileApiModel> GetAPIJobProfileDetail(RestHttpClient.RestHttpClient client, JobProfileSummaryApiModel summary)
        {
            ColorConsole.WriteLine($">>> Fetching {summary.Title} job profile", ConsoleColor.DarkYellow);
            var jobProfile = await client.Get<JobProfileApiModel>(new Uri(summary.Url, UriKind.Absolute));
            ColorConsole.WriteLine($"<<< Fetched {summary.Title} job profile", ConsoleColor.Yellow);

            return jobProfile;
        }

        private JobProfileContentItem CreateJobProfileContentItem(JobProfileApiModel apiJobProfile)
        {
            if (apiJobProfile == null || string.IsNullOrEmpty(apiJobProfile.Title))
            {
                throw new ArgumentNullException("ValidateJobProfileApiData: APIJobProfile or APIJobProfile.Title is null/empty");
            }

            //var uri = GetJobProfileUri(apiJobProfile?.Url ?? string.Empty);
            var dayToDayTasks2 = GetWhatItTakesDayToDayTasks(apiJobProfile?.WhatItTakes.DigitalSkillsLevel);
            var dayToDayTasks = GetSpreadsheetJobProfileModel(apiJobProfile.Title)?.DayToDayTasks ?? string.Empty;
            var digitalSkillsLevel = GetSpreadsheetJobProfileModel(apiJobProfile.Title)?.DigitalSkillsLevel ?? string.Empty;
            var hiddenAlternativeTitle = GetSpreadsheetJobProfileModel(apiJobProfile.Title)?.HiddenAlternativeTitle ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(apiJobProfile.WhatYouWillDo?.WorkingEnvironment?.Uniform) && (!apiJobProfile.WhatYouWillDo?.WorkingEnvironment?.Uniform?.StartsWith("You may need to wear ") ?? true))
            {
                throw new InvalidOperationException(
                    $"JobProfile {apiJobProfile.Title} does not conform to WydWorkingUniform prefix expectation");
            }

            string uniform = string.IsNullOrWhiteSpace(apiJobProfile.WhatYouWillDo?.WorkingEnvironment?.Uniform)
                ? string.Empty
                : apiJobProfile.WhatYouWillDo?.WorkingEnvironment?.Uniform?.Substring(21).Trim('.');

            if (!string.IsNullOrWhiteSpace(apiJobProfile.WhatYouWillDo?.WorkingEnvironment?.Location) && (!apiJobProfile.WhatYouWillDo?.WorkingEnvironment?.Location?.StartsWith("You could work ") ?? true))
            {
                throw new InvalidOperationException(
                    $"JobProfile {apiJobProfile.Title} does not conform to WydWorkingLocation prefix expectation");
            }

            string location = string.IsNullOrWhiteSpace(apiJobProfile.WhatYouWillDo?.WorkingEnvironment?.Location)
                ? string.Empty
                : apiJobProfile.WhatYouWillDo?.WorkingEnvironment?.Location?.Substring(15).Trim('.');

            if (!string.IsNullOrWhiteSpace(apiJobProfile.WhatYouWillDo?.WorkingEnvironment?.Environment) && (!apiJobProfile.WhatYouWillDo?.WorkingEnvironment?.Environment?.StartsWith("Your working environment may be ") ?? true))
            {
                throw new InvalidOperationException(
                    $"JobProfile {apiJobProfile.Title} does not conform to WydWorkingEnvironment prefix expectation");
            }

            string environment = string.IsNullOrWhiteSpace(apiJobProfile.WhatYouWillDo?.WorkingEnvironment?.Environment)
                ? string.Empty
                : apiJobProfile.WhatYouWillDo?.WorkingEnvironment?.Environment?.Substring(32).Trim('.');

            _refData.ONetOccCodes.ONetOccupationalCodeDictionary.TryGetValue(apiJobProfile.Title, out var oNetContentItemIds);

            oNetContentItemIds ??= _refData.ONetOccCodes.ONetOccupationalCodeDictionary[ONetOccCode.UnknownJobProfile];

            //string jobProfileWebsiteUrl = uri.Segments.Last();

            var jobProfileContentItem = new JobProfileContentItem(apiJobProfile.Title, _settings.Timestamp);
            jobProfileContentItem.PageLocationPart = GetJobProfilePageLocationPart(apiJobProfile);
            jobProfileContentItem.EponymousPart = GetJobProfilePart(apiJobProfile);


            //var jobProfileContentItem = new JobProfileContentItem(apiJobProfile.Title, _settings.Timestamp)
            //{
            //PageLocationPart = new PageLocationPart
            //{
            //    UrlName = jobProfileWebsiteUrl,
            //    FullUrl = $"/job-profiles/{jobProfileWebsiteUrl}",
            //},
            //EponymousPart = new JobProfilePart
            //{
            //    PageLocations = new TaxonomyField
            //    {
            //        TaxonomyContentItemId = "4eembshqzx66drajtdten34tc8",
            //        TermContentItemIds = new[] { "4x352kh85x7894jr7yqbt0z34b" }
            //    },

            //    // Job profile elements populated from the Job Profile API
            //    AlternativeTitle = new TextField(apiJobProfile.AlternativeTitle),
            //    Description = new HtmlField(apiJobProfile.Overview),
            //    TitleOptions = _titleOptionsFactory.Create(jobProfileWebsiteUrl),
            //    SOCCode = _socCodeContentPickerFactory.Create(apiJobProfile.Soc),
            //    SalaryStarter = new TextField(apiJobProfile.SalaryStarter),
            //    SalaryExperienced = new TextField(apiJobProfile.SalaryExperienced),
            //    MinimumHours = new NumericField(apiJobProfile.MinimumHours),
            //    MaximumHours = new NumericField(apiJobProfile.MaximumHours),
            //    WorkingHoursDetails = new TextField(apiJobProfile.WorkingHoursDetails),
            //    WorkingPattern = new TextField(apiJobProfile.WorkingPattern),
            //    WorkingPatternDetails = new TextField(apiJobProfile.WorkingPatternDetails),
            jobProfileContentItem.EponymousPart.HtbProfessionalBodies = new HtmlField(apiJobProfile.HowToBecome.MoreInformation.ProfessionalAndIndustryBodies);
            jobProfileContentItem.EponymousPart.HtbCareerTips = new HtmlField(apiJobProfile.HowToBecome.MoreInformation.CareerTips);
            jobProfileContentItem.EponymousPart.HtbFurtherInformation = new HtmlField(apiJobProfile.HowToBecome.MoreInformation.FurtherInformation);
            jobProfileContentItem.EponymousPart.HtbRegistrations = Registrations.CreateContentPicker(apiJobProfile.HowToBecome.MoreInformation.Registrations);
            jobProfileContentItem.EponymousPart.WitRestrictions = Restrictions.CreateContentPicker(apiJobProfile.WhatItTakes.RestrictionsAndRequirements.RelatedRestrictions);
            jobProfileContentItem.EponymousPart.WitOtherRequirements = OtherRequirements.CreateContentPicker(apiJobProfile.WhatItTakes.RestrictionsAndRequirements.OtherRequirements);
            jobProfileContentItem.EponymousPart.WydWorkingEnvironment = WorkingEnvironments.CreateContentPicker(environment);
            jobProfileContentItem.EponymousPart.WydWorkingLocation = WorkingLocations.CreateContentPicker(location);
            jobProfileContentItem.EponymousPart.WydWorkingUniform = WorkingUniforms.CreateContentPicker(uniform);
            //jobProfileContentItem.EponymousPart.WitDigitalSkillsLevel = DigitalSkillsLevels.CreateContentPicker(_refData.DigitalSkillsLevel.DigitalSkillLevelDictionary);

                    //CareerPathAndProgression = new HtmlField(apiJobProfile.CareerPathAndProgression.CareerPathAndProgression),
                    //RelatedCareers = null,

            // Job profile elements populated from the spreadsheet exported from Sitefinity
            //     HiddenAlternativeTitle = new TextField(hiddenAlternativeTitle),
            //     WydDayToDayTasks = new HtmlField(dayToDayTasks),

            //     // Job profile elements populated from other spreadsheets or files
            //     ONetOccupationalCode = new ContentPicker { ContentItemIds = new List<string> { oNetContentItemIds } },

            //     // Internal (hard-coded) data lists
            //     //WitDigitalSkillsLevel = DigitalSkillsLevels.CreateContentPicker(apiJobProfile.WhatItTakes.DigitalSkillsLevel),
            //     WitDigitalSkillsLevel = DigitalSkillsLevels.CreateContentPicker(_refData.DigitalSkillsLevel.DigitalSkillLevelDictionary)
            //// }
            //};

            //if (apiJobProfile.HowToBecome.EntryRoutes.Apprenticeship.IsEmpty())
            //{
            //    jobProfileContentItem.EponymousPart.ApprenticeshipRoute = new ContentPicker();
            //}
            //else
            //{
            //    var apprenticeshipEntryRoute = ApprenticeshipRoutes.Create(jobProfileContentItem.DisplayText,
            //        apiJobProfile.HowToBecome.EntryRoutes.Apprenticeship, _settings.Timestamp);

            //    jobProfileContentItem.EponymousPart.ApprenticeshipRoute = ApprenticeshipRoute.CreateContentPicker(apprenticeshipEntryRoute);
            //}

            //if (apiJobProfile.HowToBecome.EntryRoutes.College.IsEmpty())
            //{
            //    jobProfileContentItem.EponymousPart.CollegeRoute = new ContentPicker();
            //}
            //else
            //{
            //    var collegeEntryRoute = CollegeRoutes.Create(jobProfileContentItem.DisplayText,
            //        apiJobProfile.HowToBecome.EntryRoutes.College, _settings.Timestamp);

            //    jobProfileContentItem.EponymousPart.CollegeRoute = CollegeRoute.CreateContentPicker(collegeEntryRoute);
            //}

            //if (apiJobProfile.HowToBecome.EntryRoutes.University.IsEmpty())
            //{
            //    jobProfileContentItem.EponymousPart.UniversityRoute = new ContentPicker();
            //}
            //else
            //{
            //    var universityEntryRoute = UniversityRoutes.Create(
            //        jobProfileContentItem.DisplayText,
            //        apiJobProfile.HowToBecome.EntryRoutes.University,
            //        _settings.Timestamp);

            //    jobProfileContentItem.EponymousPart.UniversityRoute = UniversityRoute.CreateContentPicker(universityEntryRoute);
            //}

            //if (!apiJobProfile.HowToBecome.EntryRoutes.DirectApplication.Any())
            //{
            //    jobProfileContentItem.EponymousPart.DirectRoute = new ContentPicker();
            //}
            //else
            //{
            //    var route = new DirectRouteContentItem(jobProfileContentItem.DisplayText, _settings.Timestamp,
            //        apiJobProfile.HowToBecome.EntryRoutes.DirectApplication);

            //    jobProfileContentItem.EponymousPart.DirectRoute = DirectRoute.CreateContentPicker(route);
            //}

            //if (!apiJobProfile.HowToBecome.EntryRoutes.OtherRoutes.Any())
            //{
            //    jobProfileContentItem.EponymousPart.OtherRoute = new ContentPicker();
            //}
            //else
            //{
            //    var route = new OtherRouteContentItem(jobProfileContentItem.DisplayText, _settings.Timestamp,
            //        apiJobProfile.HowToBecome.EntryRoutes.OtherRoutes);

            //    jobProfileContentItem.EponymousPart.OtherRoute = OtherRoute.CreateContentPicker(route);
            //}

            //if (!apiJobProfile.HowToBecome.EntryRoutes.Volunteering.Any())
            //{
            //    jobProfileContentItem.EponymousPart.VolunteeringRoute = new ContentPicker();
            //}
            //else
            //{
            //    var route = new VolunteeringRouteContentItem(jobProfileContentItem.DisplayText, _settings.Timestamp,
            //        apiJobProfile.HowToBecome.EntryRoutes.Volunteering);

            //    jobProfileContentItem.EponymousPart.VolunteeringRoute = VolunteeringRoute.CreateContentPicker(route);
            //}

            //if (!apiJobProfile.HowToBecome.EntryRoutes.Work.Any())
            //{
            //    jobProfileContentItem.EponymousPart.WorkRoute = new ContentPicker();
            //}
            //else
            //{
            //    var route = new WorkRouteContentItem(jobProfileContentItem.DisplayText, _settings.Timestamp,
            //        apiJobProfile.HowToBecome.EntryRoutes.Work);

            //    jobProfileContentItem.EponymousPart.WorkRoute = WorkRoute.CreateContentPicker(route);
            //}

            // todo: This code has been replaced with an HtmlField above but kept is case we need to revert back to DayToDayTask content items
            //if (DayToDayTaskExclusions.Contains(jobProfile.Url))
            //{
            //todo: new day to day tasks stuff here
            //jobProfileContentItem.EponymousPart.DayToDayTasks = new ContentPicker();
            //}
            //else
            //{
            //    var searchTerms = new[]
            //    {
            //        "include:",
            //        "be:",
            //        "then:",
            //        "like:",
            //        "checking:",
            //        "time:",
            //        "involve:",
            //        "you’ll:",
            //        "you'll",
            //        ":using",
            //        "usually:",
            //        "in:",
            //        "to:",
            //        "by:",
            //        "with:",
            //        "are:",
            //        "might:",
            //        "could:",
            //        "types:",
            //        "a:"
            //    };

            //    if (jobProfile.WhatYouWillDo.WYDDayToDayTasks.All(x => !searchTerms.Any(t => x.Contains(t, StringComparison.OrdinalIgnoreCase))))
            //        DayToDayTaskExclusions.Add(jobProfile.Url);

            //    var activities = jobProfile.WhatYouWillDo.WYDDayToDayTasks
            //        .Where(x => searchTerms.Any(t => x.Contains(t, StringComparison.OrdinalIgnoreCase)))
            //        .SelectMany(a => a.Substring(a.IndexOf(":") + 1).Split(";")).Select(x => x.Trim())
            //        .ToList();

            //    foreach (var activity in activities)
            //    {
            //        if (!DayToDayTasks.TryAdd(activity, (_idGenerator.GenerateUniqueId(), activity)))
            //        {
            //            ColorConsole.WriteLine($"DayToDayTask '{activity}' already saved", ConsoleColor.Red);
            //        }
            //    }

            //    jobProfileContentItem.EponymousPart.DayToDayTasks = new ContentPicker(DayToDayTasks, activities);
            //}

            return jobProfileContentItem;
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

        private PageLocationPart GetJobProfilePageLocationPart(JobProfileApiModel apiJobProfile)
        {
            var uri = GetJobProfileUri(apiJobProfile?.Url);

            string jobProfileWebsiteUrl = uri.Segments.Last();

            if(string.IsNullOrEmpty(jobProfileWebsiteUrl))
            {
                throw new ArgumentNullException("$JobProfile { apiJobProfile.Title } jobProfileWebsiteUrl is null");
            }

            var pageLocationPart = new PageLocationPart
            {
                UrlName = jobProfileWebsiteUrl,
                FullUrl = $"/job-profiles/{jobProfileWebsiteUrl}",
            };

            return pageLocationPart;
        }

        private JobProfilePart GetJobProfilePart(JobProfileApiModel jobProfileApiModel)
        {
            var jobProfilePart = new JobProfilePart();

            jobProfilePart.PageLocations = new TaxonomyField
            {
                TaxonomyContentItemId = "4eembshqzx66drajtdten34tc8",
                TermContentItemIds = new[] { "4x352kh85x7894jr7yqbt0z34b" }
            };

            // Reference data (Content Pickers)
            jobProfilePart.SOCCode = GetSocCodeContentPicker(jobProfileApiModel.Soc);
            jobProfilePart.ONetOccupationalCode = GetOnetOccupationalCodeContentPicker(jobProfileApiModel.Title);
            jobProfilePart.ApprenticeshipRoute = GetApprenticeshipRouteContentPicker(jobProfileApiModel);
            jobProfilePart.CollegeRoute = GetCollegeRouteContentPicker(jobProfileApiModel);
            jobProfilePart.UniversityRoute = GetUniversityRouteContentPicker(jobProfileApiModel);
            jobProfilePart.DirectRoute = GetDirectRouteContentPicker(jobProfileApiModel);
            jobProfilePart.OtherRoute = GetOtherRouteContentPicker(jobProfileApiModel);
            jobProfilePart.VolunteeringRoute = GetVolunteeringRouteContentPicker(jobProfileApiModel);
            jobProfilePart.WorkRoute = GetWorkRouteContentPicker(jobProfileApiModel);
            jobProfilePart.HtbRegistrations = GetRegistrationsContentPicker(jobProfileApiModel.HowToBecome.MoreInformation.Registrations);
            jobProfilePart.WitDigitalSkillsLevel = GetDigitalSkillsLevelContentPicker(jobProfileApiModel.WhatItTakes.DigitalSkillsLevel);

            // Fields
            jobProfilePart.AlternativeTitle = GetJobProfileAlternativeTitleTextField(jobProfileApiModel.AlternativeTitle);
            jobProfilePart.HiddenAlternativeTitle = GetJobProfileHiddenAlternativeTitleTextField(jobProfileApiModel.Title);
            jobProfilePart.Description = GetJobProfileDescriptionHtmlField(jobProfileApiModel.Overview);
            jobProfilePart.TitleOptions = GetJobProfileTitleOptionsTextField(jobProfileApiModel.Url);
            jobProfilePart.SalaryStarter = GetJobProfileSalaryStarterTextField(jobProfileApiModel.SalaryStarter);
            jobProfilePart.SalaryExperienced = GetJobProfileSalaryExperiencedTextField(jobProfileApiModel.SalaryExperienced);
            jobProfilePart.MinimumHours = GetJobProfileMinimumHoursNumericField(jobProfileApiModel.MinimumHours);
            jobProfilePart.MaximumHours = GetJobProfileMaximumHoursNumericField(jobProfileApiModel.MaximumHours);
            jobProfilePart.WorkingHoursDetails = GetJobProfileWorkingHoursDetailsTextField(jobProfileApiModel.WorkingHoursDetails);
            jobProfilePart.WorkingPattern = GetJobProfileWorkingPatternTextField(jobProfileApiModel.WorkingPattern);
            jobProfilePart.WorkingPatternDetails = GetJobProfileWorkingPatternDetailsTextField(jobProfileApiModel.WorkingPatternDetails);

            //jobProfilePart.HtbProfessionalBodies = GetJobProfileHtbProfessionalBodiesHtmlField(jobProfileApiModel.HowToBecome.MoreInformation.ProfessionalAndIndustryBodies);
            //jobProfilePart.HtbCareerTips = GetJobProfileHtbCareerTipsHtmlField(jobProfileApiModel.HowToBecome.MoreInformation.CareerTips);
            //jobProfilePart.HtbFurtherInformation = GetJobProfileHtbFurtherInformationHtmlField(jobProfileApiModel.HowToBecome.MoreInformation.FurtherInformation);
            //        WitRestrictions = Restrictions.CreateContentPicker(apiJobProfile.WhatItTakes.RestrictionsAndRequirements.RelatedRestrictions),
            //        WitOtherRequirements = OtherRequirements.CreateContentPicker(apiJobProfile.WhatItTakes.RestrictionsAndRequirements.OtherRequirements),
            //        WydWorkingEnvironment = WorkingEnvironments.CreateContentPicker(environment),
            //        WydWorkingLocation = WorkingLocations.CreateContentPicker(location),
            //        WydWorkingUniform = WorkingUniforms.CreateContentPicker(uniform),
            //        CareerPathAndProgression = new HtmlField(apiJobProfile.CareerPathAndProgression.CareerPathAndProgression),

            jobProfilePart.RelatedCareers = null;

            return jobProfilePart;
        }

        // Reference Data methods

        private ContentPicker GetSocCodeContentPicker(string socCode)
        {
            // Should the source of this data really come from the Job Profile API?
            // TODO: Check the source of the data for the Soc Code picker
            // I would expect it to come from the available Soc codes list.

            return _socCodeContentPickerFactory.Create(socCode);
        }

        private ContentPicker GetOnetOccupationalCodeContentPicker(string title)
        {
            _refData.ONetOccCodes.ONetOccupationalCodeDictionary.TryGetValue(title, out var oNetContentItemIds);
            oNetContentItemIds ??= _refData.ONetOccCodes.ONetOccupationalCodeDictionary[ONetOccCode.UnknownJobProfile];

            return new ContentPicker { ContentItemIds = new List<string> { oNetContentItemIds } };
        }

        private ContentPicker GetApprenticeshipRouteContentPicker(JobProfileApiModel jobProfileApiModel)
        {
            if (jobProfileApiModel.HowToBecome.EntryRoutes.Apprenticeship.IsEmpty())
            {
                return new ContentPicker();
            }
            else
            {
                var titlePart = new TitlePart(jobProfileApiModel.Title);

                // ApprenticeshipRoutes is a Public ApprenticeshipRouteFactory() TODO: refactor to pass in via contructor instead of public variable
                var apprenticeshipEntryRoute = ApprenticeshipRoutes.Create(
                    titlePart.Title,
                    jobProfileApiModel.HowToBecome.EntryRoutes.Apprenticeship,
                    _settings.Timestamp);

                return ApprenticeshipRoute.CreateContentPicker(apprenticeshipEntryRoute);
            }
        }

        private ContentPicker GetCollegeRouteContentPicker(JobProfileApiModel jobProfileApiModel)
        {
            if (jobProfileApiModel.HowToBecome.EntryRoutes.College.IsEmpty())
            {
                return new ContentPicker();
            }
            else
            {
                var titlePart = new TitlePart(jobProfileApiModel.Title);

                // CollegeRoutes is a Public CollegeRouteFactory() TODO: refactor to pass in via contructor instead of public variable
                var collegeEntryRoute = CollegeRoutes.Create(
                    titlePart.Title,
                    jobProfileApiModel.HowToBecome.EntryRoutes.College,
                    _settings.Timestamp);

                return CollegeRoute.CreateContentPicker(collegeEntryRoute);
            }
        }

        private ContentPicker GetUniversityRouteContentPicker(JobProfileApiModel jobProfileApiModel)
        {
            if (jobProfileApiModel.HowToBecome.EntryRoutes.University.IsEmpty())
            {
                return new ContentPicker();
            }
            else
            {
                var titlePart = new TitlePart(jobProfileApiModel.Title);

                // UniversityRoutes is a Public UniversityRouteFactory() TODO: refactor to pass in via contructor instead of public variable
                var universityEntryRoute = UniversityRoutes.Create(
                    titlePart.Title,
                    jobProfileApiModel.HowToBecome.EntryRoutes.University,
                    _settings.Timestamp);

                return UniversityRoute.CreateContentPicker(universityEntryRoute);
            }
        }

        private ContentPicker GetDirectRouteContentPicker(JobProfileApiModel jobProfileApiModel)
        {
            if (!jobProfileApiModel.HowToBecome.EntryRoutes.DirectApplication.Any())
            {
                return new ContentPicker();
            }
            else
            {
                var titlePart = new TitlePart(jobProfileApiModel.Title);

                // DirectRoutes is a Public DirectRouteFactory() TODO: refactor to pass in via contructor instead of public variable
                var directRoute = new DirectRouteContentItem(
                    titlePart.Title,
                    _settings.Timestamp,
                    jobProfileApiModel.HowToBecome.EntryRoutes.DirectApplication);

                return DirectRoute.CreateContentPicker(directRoute);
            }
        }

        private ContentPicker GetOtherRouteContentPicker(JobProfileApiModel jobProfileApiModel)
        {
            if (!jobProfileApiModel.HowToBecome.EntryRoutes.OtherRoutes.Any())
            {
                return new ContentPicker();
            }
            else
            {
                var titlePart = new TitlePart(jobProfileApiModel.Title);

                // OtherRoutes is a Public OtherRouteFactory() TODO: refactor to pass in via contructor instead of public variable
                var otherRoute = new OtherRouteContentItem(
                    titlePart.Title,
                    _settings.Timestamp,
                    jobProfileApiModel.HowToBecome.EntryRoutes.OtherRoutes);

                return OtherRoute.CreateContentPicker(otherRoute);
            }
        }

        private ContentPicker GetVolunteeringRouteContentPicker(JobProfileApiModel jobProfileApiModel)
        {
            if (!jobProfileApiModel.HowToBecome.EntryRoutes.Volunteering.Any())
            {
                return new ContentPicker();
            }
            else
            {
                var titlePart = new TitlePart(jobProfileApiModel.Title);

                // VolunteeringRoutes is a Public VolunteeringRouteFactory() TODO: refactor to pass in via contructor instead of public variable
                var volunteeringRoute = new VolunteeringRouteContentItem(
                    titlePart.Title,
                    _settings.Timestamp,
                    jobProfileApiModel.HowToBecome.EntryRoutes.Volunteering);

                return VolunteeringRoute.CreateContentPicker(volunteeringRoute);
            }
        }

        private ContentPicker GetWorkRouteContentPicker(JobProfileApiModel jobProfileApiModel)
        {
            if (!jobProfileApiModel.HowToBecome.EntryRoutes.Work.Any())
            {
                return new ContentPicker();
            }
            else
            {
                var titlePart = new TitlePart(jobProfileApiModel.Title);

                // WorkRoutes is a Public WorkRouteFactory() TODO: refactor to pass in via contructor instead of public variable
                var workRoute = new WorkRouteContentItem(
                    titlePart.Title,
                    _settings.Timestamp,
                    jobProfileApiModel.HowToBecome.EntryRoutes.Work);

                return WorkRoute.CreateContentPicker(workRoute);
            }
        }

        private ContentPicker GetRegistrationsContentPicker(string[] registrations)
        {
            // Registrations is a Public ContentPickerFactory TODO: Refactor to pass in via contructor
            return Registrations.CreateContentPicker(registrations);
        }

        private ContentPicker GetDigitalSkillsLevelContentPicker(string digitalSkillsLevel)
        {
            return DigitalSkillsLevels.CreateContentPicker(digitalSkillsLevel);
        }

        // Field data methods

        private TextField GetJobProfileAlternativeTitleTextField(string alternativeTitle)
        {
            return new TextField(alternativeTitle);
        }

        private TextField GetJobProfileHiddenAlternativeTitleTextField(string title)
        {
            var hiddenAlternativeTitle = GetSpreadsheetJobProfileModel(title)?.HiddenAlternativeTitle ?? string.Empty;

            return new TextField(hiddenAlternativeTitle);
        }

        private HtmlField GetJobProfileDescriptionHtmlField(string description)
        {
            return new HtmlField(description);
        }

        private TextField GetJobProfileSalaryStarterTextField(string salaryStarter)
        {
            return new TextField(salaryStarter);
        }

        private TextField GetJobProfileSalaryExperiencedTextField(string salaryExperienced)
        {
            return new TextField(salaryExperienced);
        }

        private NumericField GetJobProfileMinimumHoursNumericField(decimal minimumHours)
        {
            return new NumericField(minimumHours);
        }

        private NumericField GetJobProfileMaximumHoursNumericField(decimal maximumHours)
        {
            return new NumericField(maximumHours);
        }

        private TextField GetJobProfileWorkingHoursDetailsTextField(string workingHoursDetails)
        {
            return new TextField(workingHoursDetails);
        }

        private TextField GetJobProfileWorkingPatternTextField(string workingPattern)
        {
            return new TextField(workingPattern);
        }

        private TextField GetJobProfileWorkingPatternDetailsTextField(string workingPatternDetails)
        {
            return new TextField(workingPatternDetails);
        }

        private HtmlField GetJobProfileCareerPathAndProgressionHtmlField(string[] careerPathAndProgression)
        {
            return new HtmlField(careerPathAndProgression);
        }

        private TextField GetJobProfileTitleOptionsTextField(string url)
        {
            var uri = GetJobProfileUri(url);

            string jobProfileWebsiteUrl = uri.Segments.Last();

            if (string.IsNullOrEmpty(jobProfileWebsiteUrl))
            {
                throw new ArgumentNullException("$JobProfile { apiJobProfile.Title } jobProfileWebsiteUrl is null");
            }

            var titleOptionsTextField = _titleOptionsFactory.Create(jobProfileWebsiteUrl);

            if (titleOptionsTextField == null)
            {
                throw new ArgumentNullException("$JobProfile { apiJobProfile.Title } titleOptionsTextField is null");
            }

            return titleOptionsTextField;
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

        private RestHttpClient.RestHttpClient GetJobProfileApiRestHttpClient(SettingsModel jobProfileSettingsDataModel)
        {
            if (jobProfileSettingsDataModel == null || jobProfileSettingsDataModel.JobProfileApiUri == null || jobProfileSettingsDataModel.AppSettings == null)
            {
                throw new ArgumentNullException("jobProfileSettingsDataModel is null");
            }

            var jobProfileApiHttpClient = new HttpClient
            {
                BaseAddress = new Uri(jobProfileSettingsDataModel.JobProfileApiUri),
                DefaultRequestHeaders =
                {
                    {"version", "v1"},
                    {"Ocp-Apim-Subscription-Key", jobProfileSettingsDataModel.AppSettings["Ocp-Apim-Subscription-Key"]}
                }
            };

            if (jobProfileApiHttpClient == null)
            {
                throw new Exception("Failed to get a new HttpClient for the JobProfiles API connection");
            }

            var jobProfileApiRestHttpClient = new RestHttpClient.RestHttpClient(jobProfileApiHttpClient);

            if (jobProfileApiHttpClient == null)
            {
                throw new Exception("Failed to get a new RestHttpClient for the JobProfiles API connection");
            }

            return jobProfileApiRestHttpClient;
        }

        private Uri GetJobProfileUri(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentNullException("GetProfileUri: uri is null or empty");
            }

            return new Uri(uri);
        }

        private string GetWhatItTakesDayToDayTasks(string dayToDayTasks)
        {

            return dayToDayTasks;
        }

        private JobProfileExcelWorkbookColumnsModel GetSpreadsheetJobProfileModel(string key)
        {
            JobProfileExcelWorkbookColumnsModel jobProfileExcelWorkbookColumnsModel = null;
            key = key.Replace(" ", "-").ToLower();

            if (jobProfileExcelWorkbookColumnsDataModel.TryGetValue(key, out jobProfileExcelWorkbookColumnsModel) == true)
            {
                return jobProfileExcelWorkbookColumnsModel;
            }

            return null;
        }

        private async Task<IEnumerable<IEnumerable<JobProfileSummaryApiModel>>> GetApiJobProfiles(string jobProfilesToImportCsv, int skip, int take, int napTimeMs)
        {
            IEnumerable<JobProfileSummaryApiModel> jobProfileSummaries;
            EscoJobProfileMapper exclusionSource = new EscoJobProfileMapper();

            if (!string.IsNullOrWhiteSpace(jobProfilesToImportCsv) && jobProfilesToImportCsv != "*")
            {
                string[] jobProfilesToImportUntrimmed = jobProfilesToImportCsv.Split(",");
                HashSet<string> jobProfilesToImport = new HashSet<string>(jobProfilesToImportUntrimmed.Select(jp => jp.Trim().ToLowerInvariant()));

                jobProfileSummaries = (await _jobProfileApiRestHttpClient.Get<JobProfileSummaryApiModel[]>("summary"))
                    .Where(s => s.Title != null &&
                        jobProfilesToImport.Contains(s.Title.ToLowerInvariant()) &&
                        !exclusionSource._exclusions.Contains(s.Title.ToLowerInvariant().Replace(" ", "-")));
            }
            else
            {
                jobProfileSummaries = (await _jobProfileApiRestHttpClient.Get<JobProfileSummaryApiModel[]>("summary"))
                                        .Where(s => s.Title != null &&
                                        !exclusionSource._exclusions.Contains(s.Title.ToLowerInvariant().Replace(" ", "-")));
            }

            if (skip > 0)
                jobProfileSummaries = jobProfileSummaries.Skip(skip);
            if (take > 0)
                jobProfileSummaries = jobProfileSummaries.Take(take);

            return jobProfileSummaries.Batch(10);
        }
    }
}
