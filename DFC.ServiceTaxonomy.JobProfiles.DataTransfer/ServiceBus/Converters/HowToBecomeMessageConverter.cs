using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Converters
{
    public class HowToBecomeMessageConverter : IMessageConverter<HowToBecomeData>
    {
        private readonly IServiceProvider _serviceProvider;

        public HowToBecomeMessageConverter(IServiceProvider serviceProvider) =>
            _serviceProvider = serviceProvider;

        public async Task<HowToBecomeData> ConvertFromAsync(ContentItem contentItem)
        {
            var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
            IEnumerable<ContentItem> universityEntryRequirements = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.UniversityEntryRequirements, contentManager);
            IEnumerable<ContentItem> relatedUniversityRequirements = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.RelatedUniversityRequirements, contentManager);
            IEnumerable<ContentItem> relatedUniversityLinks = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.RelatedUniversityLinks, contentManager);
            IEnumerable<ContentItem> collegeEntryRequirements = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.CollegeEntryRequirements, contentManager);
            IEnumerable<ContentItem> relatedCollegeRequirements = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.RelatedCollegeRequirements, contentManager);
            IEnumerable<ContentItem> relatedCollegeLinks = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.RelatedCollegeLinks, contentManager);
            IEnumerable<ContentItem> apprenticeshipEntryRequirements = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.ApprenticeshipEntryRequirements, contentManager);
            IEnumerable<ContentItem> relatedApprenticeshipRequirements = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.RelatedApprenticeshipRequirements, contentManager);
            IEnumerable<ContentItem> relatedApprenticeshipLinks = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.RelatedApprenticeshipLinks, contentManager);
            IEnumerable<ContentItem> relatedRegistrations = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.RelatedRegistrations, contentManager);
            IEnumerable<ContentItem> realStoryField = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.RealStory, contentManager);
            
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
                Registrations = GetRegistrations(relatedRegistrations),
                RealStory = GetRealStory(realStoryField),
            };
            return howToBecomeData;
        }

        private static RealStory GetRealStory(ContentItem contentItem)
        {
            var thumbnail = contentItem.Content.RealStory.Thumbnail.Paths[0] is null ? default : new Thumbnail(
                path: (string)contentItem.Content.RealStory.Thumbnail.Paths[0],
                text: (string)contentItem.Content.RealStory.Thumbnail.MediaTexts[0]
            );

            return new RealStory(
                title: contentItem.DisplayText,
                thumbnail: thumbnail,
                summary: (string)contentItem.Content.RealStory.Summary.Text,
                bodyHtml: (string)contentItem.Content.RealStory.Body.Html
            );
        }

        private static RealStory? GetRealStory(IEnumerable<ContentItem> contentItems)
        {
            var contentItem = contentItems.FirstOrDefault();
            return contentItem == null ? default : GetRealStory(contentItem);
        }

        private static IEnumerable<RegistrationItem> GetRegistrations(IEnumerable<ContentItem> contentItems) =>
            contentItems?.Select(contentItem => new RegistrationItem
            {
                Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                Title = contentItem.As<TitlePart>().Title,
                Info = contentItem.Content.Registration.Info.Html
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
                ContentTypes.UniversityLink => contentItem.Content.UniversityLink.URL.Text,
                ContentTypes.CollegeLink => contentItem.Content.CollegeLink.URL.Text,
                ContentTypes.ApprenticeshipLink => contentItem.Content.ApprenticeshipLink.URL.Text,
                _ => string.Empty,
            };

            return !string.IsNullOrWhiteSpace(link.Value) ? new Uri(link.Value, UriKind.RelativeOrAbsolute) : default;
        }

        private static string GetRelatedLinkText(ContentItem contentItem) =>
            contentItem.ContentType switch
            {
                ContentTypes.UniversityLink => contentItem.Content.UniversityLink.Text.Text,
                ContentTypes.CollegeLink => contentItem.Content.CollegeLink.Text.Text,
                ContentTypes.ApprenticeshipLink => contentItem.Content.ApprenticeshipLink.Text.Text,
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
                ContentTypes.UniversityRequirements => contentItem.Content.UniversityRequirements.Info.Html,
                ContentTypes.CollegeRequirements => contentItem.Content.CollegeRequirements.Info.Html,
                ContentTypes.ApprenticeshipRequirements => contentItem.Content.ApprenticeshipRequirements.Info.Html,
                _ => string.Empty,
            };
    }
}
