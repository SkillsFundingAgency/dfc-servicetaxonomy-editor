using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters
{
    public class HowToBecomeMessageConverter : IMessageConverter<HowToBecomeData>
    {
        private readonly IServiceProvider _serviceProvider;

        public HowToBecomeMessageConverter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public  HowToBecomeData ConvertFrom(ContentItem contentItem)
        {
            var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
            List<ContentItem> universityEntryRequirements =  GetContentItems(contentItem.Content.JobProfile.Universityentryrequirements, contentManager);
            List<ContentItem> relatedUniversityRequirements =  GetContentItems(contentItem.Content.JobProfile.Relateduniversityrequirements, contentManager);
            List<ContentItem> relatedUniversityLinks =  GetContentItems(contentItem.Content.JobProfile.Relateduniversitylinks, contentManager);
            List<ContentItem> collegeEntryRequirements =  GetContentItems(contentItem.Content.JobProfile.Collegeentryrequirements, contentManager);
            List<ContentItem> relatedCollegeRequirements =  GetContentItems(contentItem.Content.JobProfile.Relatedcollegerequirements, contentManager);
            List<ContentItem> relatedCollegeLinks =  GetContentItems(contentItem.Content.JobProfile.Relatedcollegelinks, contentManager);
            List<ContentItem> apprenticeshipEntryRequirements =  GetContentItems(contentItem.Content.JobProfile.Apprenticeshipentryrequirements, contentManager);
            List<ContentItem> relatedApprenticeshipRequirements =  GetContentItems(contentItem.Content.JobProfile.Relatedapprenticeshiprequirements, contentManager);
            List<ContentItem> relatedApprenticeshipLinks =  GetContentItems(contentItem.Content.JobProfile.Relatedapprenticeshiplinks, contentManager);
            List<ContentItem> relatedRegistrations =  GetContentItems(contentItem.Content.JobProfile.Relatedregistrations, contentManager);


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
                        RouteRequirement = universityEntryRequirements.FirstOrDefault()?.As<TitlePart>().Title ?? String.Empty,
                        EntryRequirements = GetEntryRequirements(relatedUniversityRequirements),
                        MoreInformationLinks = GetRelatedLinkItems(relatedUniversityLinks)
                    },

                    // College
                    new RouteEntryItem
                    {
                        RouteName = RouteEntryType.College,
                        RouteSubjects = contentItem.Content.JobProfile.Collegerelevantsubjects.Html,
                        FurtherRouteInformation = contentItem.Content.JobProfile.Collegefurtherrouteinfo.Html,
                        RouteRequirement = collegeEntryRequirements.FirstOrDefault()?.As<TitlePart>().Title ?? String.Empty,
                        EntryRequirements = GetEntryRequirements(relatedCollegeRequirements),
                        MoreInformationLinks = GetRelatedLinkItems(relatedCollegeLinks)

                    },

                    // Apprenticeship
                    new RouteEntryItem
                    {
                        RouteName = RouteEntryType.Apprenticeship,
                        RouteSubjects = contentItem.Content.JobProfile.Apprenticeshiprelevantsubjects.Html,
                        FurtherRouteInformation = contentItem.Content.JobProfile.Apprenticeshipfurtherroutesinfo.Html,
                        RouteRequirement = apprenticeshipEntryRequirements.FirstOrDefault()?.As<TitlePart>().Title ?? String.Empty,
                        EntryRequirements = GetEntryRequirements(relatedApprenticeshipRequirements),
                        MoreInformationLinks = GetRelatedLinkItems(relatedApprenticeshipLinks)
                    }
                },
                Registrations = GetRegistrations(relatedRegistrations)
            };
            return howToBecomeData;
        }

        private IEnumerable<RegistrationItem> GetRegistrations(List<ContentItem> contentItems)
        {
            var requirements = new List<RegistrationItem>();
            if (contentItems.Any())
            {
                foreach (var contentItem in contentItems)
                {
                    requirements.Add(new RegistrationItem
                    {
                        Title = contentItem.As<TitlePart>().Title,
                        Info = contentItem.Content.Registration.Description.Html
                    });
                }
            }

            return requirements;
        }

        private IEnumerable<MoreInformationLinkItem> GetRelatedLinkItems(List<ContentItem> contentItems)
        {
            var linkItems = new List<MoreInformationLinkItem>();
            if (contentItems.Any())
            {
                foreach (var contentItem in contentItems)
                {
                    var link = GetRelatedLinkURL(contentItem);
                    linkItems.Add(new MoreInformationLinkItem
                    {
                        Title = contentItem.As<TitlePart>().Title,
                        Url = !string.IsNullOrWhiteSpace(link) ? new Uri(link, UriKind.RelativeOrAbsolute) : default,
                        Text = GetRelatedLinkText(contentItem)
                    });
                }
            }

            return linkItems;
        }

        private string GetRelatedLinkURL(ContentItem contentItem)
        {
            switch (contentItem.ContentType)
            {
                case "Universitylink":
                    return contentItem.Content.Universitylinks.URL.Text;
                case "Collegelink":
                    return contentItem.Content.Collegelink.URL.Text;
                case "Apprenticeshiplink":
                    return contentItem.Content.Apprenticeshiplink.URL.Text;
                default: return string.Empty;
            }
        }

        private string GetRelatedLinkText(ContentItem contentItem)
        {
            switch (contentItem.ContentType)
            {
                case "Universitylink":
                    return contentItem.Content.Universitylinks.Text.Text;
                case "Collegelink":
                    return contentItem.Content.Collegelink.Text.Text;
                case "Apprenticeshiplink":
                    return contentItem.Content.Apprenticeshiplink.Text.Text;
                default: return string.Empty;
            }
        }

        private IEnumerable<EntryRequirementItem> GetEntryRequirements(List<ContentItem> contentItems)
        {
            var requirements = new List<EntryRequirementItem>();
            if (contentItems.Any())
            {
                foreach (var contentItem in contentItems)
                {
                    requirements.Add(new EntryRequirementItem
                    {
                        Title = contentItem.As<TitlePart>().Title,
                        Info = GetEntryRequirementInfo(contentItem)
                    });
                }
            }

            return requirements;
        }

        private string GetEntryRequirementInfo(ContentItem contentItem)
        {
            switch(contentItem.ContentType)
            {
                case "Universityrequirements":
                    return contentItem.Content.Universityrequirements.Info.Html;
                case "Collegerequirements":
                    return contentItem.Content.Collegerequirements.Info.Html;
                case "Apprenticeshiprequirements":
                    return contentItem.Content.Apprenticeshiprequirements.Info.Html;
                default: return string.Empty;
            }
        }

        private  List<ContentItem> GetContentItems(dynamic contentPicker, IContentManager contentManager)
        {
            var contentItemIds = (JArray)contentPicker.ContentItemIds;
            if (contentItemIds.Any())
            {
                var idList = contentItemIds.Select(c => c.Value<string>()).ToList();
                var contentItems = contentManager.GetAsync(idList).Result;
                return contentItems.ToList();
            }

            return new List<ContentItem>();
        }
    }
}
