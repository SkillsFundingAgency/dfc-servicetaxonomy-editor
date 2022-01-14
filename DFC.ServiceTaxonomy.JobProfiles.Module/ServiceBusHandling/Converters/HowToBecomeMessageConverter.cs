using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters
{
    public class HowToBecomeMessageConverter : IMessageConverter<HowToBecomeData>
    {
        private readonly IServiceProvider _serviceProvider;

        public HowToBecomeMessageConverter(IServiceProvider serviceProvider) =>
            _serviceProvider = serviceProvider;

        public async Task<HowToBecomeData> ConvertFromAsync(ContentItem contentItem)
        {
            var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
            IEnumerable<ContentItem> universityEntryRequirements = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Universityentryrequirements, contentManager);
            IEnumerable<ContentItem> relatedUniversityRequirements = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Relateduniversityrequirements, contentManager);
            IEnumerable<ContentItem> relatedUniversityLinks = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Relateduniversitylinks, contentManager);
            IEnumerable<ContentItem> collegeEntryRequirements = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Collegeentryrequirements, contentManager);
            IEnumerable<ContentItem> relatedCollegeRequirements = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Relatedcollegerequirements, contentManager);
            IEnumerable<ContentItem> relatedCollegeLinks = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Relatedcollegelinks, contentManager);
            IEnumerable<ContentItem> apprenticeshipEntryRequirements = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Apprenticeshipentryrequirements, contentManager);
            IEnumerable<ContentItem> relatedApprenticeshipRequirements = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Relatedapprenticeshiprequirements, contentManager);
            IEnumerable<ContentItem> relatedApprenticeshipLinks = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Relatedapprenticeshiplinks, contentManager);
            IEnumerable<ContentItem> relatedRegistrations = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.Relatedregistrations, contentManager);


            var howToBecomeData = new HowToBecomeData
            {
                IntroText = contentItem.Content.JobProfile.Entryroutes.Html,
                FurtherRoutes = new FurtherRoutes
                {
                    Work = contentItem.Content.JobProfile.Work.Html,
                    Volunteering = contentItem.Content.JobProfile.Volunteering.Html,
                    DirectApplication = contentItem.Content.JobProfile.Directapplication.Html,
                    OtherRoutes = contentItem.Content.JobProfile.Otherroutes.Html
                },
                FurtherInformation = new MoreInformation
                {
                    ProfessionalAndIndustryBodies = contentItem.Content.JobProfile.Professionalandindustrybodies.Html,
                    CareerTips = contentItem.Content.JobProfile.Careertips.Html,
                    FurtherInformation = contentItem.Content.JobProfile.Furtherinformation.Html,
                },
                RouteEntries = new List<RouteEntryItem>
                {
                    // UNIVERSITY
                    new RouteEntryItem
                    {
                        RouteName = RouteEntryType.University,
                        RouteSubjects = contentItem.Content.JobProfile.Universityrelevantsubjects.Html,
                        FurtherRouteInformation = contentItem.Content.JobProfile.Universityfurtherrouteinfo.Html,
                        RouteRequirement = universityEntryRequirements.FirstOrDefault()?.As<TitlePart>().Title ?? string.Empty,
                        EntryRequirements = GetEntryRequirements(relatedUniversityRequirements),
                        MoreInformationLinks = GetRelatedLinkItems(relatedUniversityLinks)
                    },

                    // College
                    new RouteEntryItem
                    {
                        RouteName = RouteEntryType.College,
                        RouteSubjects = contentItem.Content.JobProfile.Collegerelevantsubjects.Html,
                        FurtherRouteInformation = contentItem.Content.JobProfile.Collegefurtherrouteinfo.Html,
                        RouteRequirement = collegeEntryRequirements.FirstOrDefault()?.As<TitlePart>().Title ?? string.Empty,
                        EntryRequirements = GetEntryRequirements(relatedCollegeRequirements),
                        MoreInformationLinks = GetRelatedLinkItems(relatedCollegeLinks)

                    },

                    // Apprenticeship
                    new RouteEntryItem
                    {
                        RouteName = RouteEntryType.Apprenticeship,
                        RouteSubjects = contentItem.Content.JobProfile.Apprenticeshiprelevantsubjects.Html,
                        FurtherRouteInformation = contentItem.Content.JobProfile.Apprenticeshipfurtherroutesinfo.Html,
                        RouteRequirement = apprenticeshipEntryRequirements.FirstOrDefault()?.As<TitlePart>().Title ?? string.Empty,
                        EntryRequirements = GetEntryRequirements(relatedApprenticeshipRequirements),
                        MoreInformationLinks = GetRelatedLinkItems(relatedApprenticeshipLinks)
                    }
                },
                Registrations = GetRegistrations(relatedRegistrations)
            };
            return howToBecomeData;
        }

        private static IEnumerable<RegistrationItem> GetRegistrations(IEnumerable<ContentItem> contentItems) =>
            contentItems?.Select(contentItem => new RegistrationItem
            {
                Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                Title = contentItem.As<TitlePart>().Title,
                Info = contentItem.Content.Registration.Description.Html
            }) ?? Enumerable.Empty<RegistrationItem>();

        private static IEnumerable<MoreInformationLinkItem> GetRelatedLinkItems(IEnumerable<ContentItem> contentItems) =>
            contentItems?.Select(contentItem =>
            {
                return new MoreInformationLinkItem
                {
                    Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                    Title = contentItem.As<TitlePart>().Title,
                    Url = GetRelatedLinkURL(contentItem),
                    Text = GetRelatedLinkText(contentItem)
                };
            }) ?? Enumerable.Empty<MoreInformationLinkItem>();


        private static Uri? GetRelatedLinkURL(ContentItem contentItem)
        {
            var link = contentItem.ContentType switch
            {
                ContentTypes.Universitylink => contentItem.Content.Universitylink.URL.Text,
                ContentTypes.Collegelink => contentItem.Content.Collegelink.URL.Text,
                ContentTypes.Apprenticeshiplink => contentItem.Content.Apprenticeshiplink.URL.Text,
                _ => string.Empty,
            };

            return !string.IsNullOrWhiteSpace(link.Value) ? new Uri(link.Value, UriKind.RelativeOrAbsolute) : default;
        }

        private static string GetRelatedLinkText(ContentItem contentItem) =>
            contentItem.ContentType switch
            {
                ContentTypes.Universitylink => contentItem.Content.Universitylink.Text.Text,
                ContentTypes.Collegelink => contentItem.Content.Collegelink.Text.Text,
                ContentTypes.Apprenticeshiplink => contentItem.Content.Apprenticeshiplink.Text.Text,
                _ => string.Empty,
            };

        private static IEnumerable<EntryRequirementItem> GetEntryRequirements(IEnumerable<ContentItem> contentItems) =>
            contentItems?.Select(contentItem => new EntryRequirementItem
            {
                Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                Title = contentItem.As<TitlePart>().Title,
                Info = GetEntryRequirementInfo(contentItem)
            }) ?? Enumerable.Empty<EntryRequirementItem>();

        private static string GetEntryRequirementInfo(ContentItem contentItem) =>
            contentItem.ContentType switch
            {
                ContentTypes.Universityrequirements => contentItem.Content.Universityrequirements.Info.Html,
                ContentTypes.Collegerequirements => contentItem.Content.Collegerequirements.Info.Html,
                ContentTypes.Apprenticeshiprequirements => contentItem.Content.Apprenticeshiprequirements.Info.Html,
                _ => string.Empty,
            };
    }
}
