using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.Indexes;
using DFC.ServiceTaxonomy.JobProfiles.Module.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;

using Microsoft.Extensions.Logging;

using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

using YesSql;
using DFC.ServiceTaxonomy.DataAccess.Repositories;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling
{
    internal class DataEventProcessor : IDataEventProcessor
    {
        private readonly IServiceBusMessageProcessor _serviceBusMessageProcessor;
        private readonly ISession _session;
        private readonly ILogger<DataEventProcessor> _logger;
        private readonly IMessageConverter<JobProfileMessage> _jobprofileMessageConverter;
        private readonly IGenericIndexRepository<JobProfileIndex> _jobProfileIndexRepository;

public DataEventProcessor(IServiceBusMessageProcessor serviceBusMessageProcessor,
                                    ILogger<DataEventProcessor> logger,
                                    IMessageConverter<JobProfileMessage> jobprofileMessageConverter,
                                    ISession session,
                                    IGenericIndexRepository<JobProfileIndex> jobProfileIndexRepository)
        {
            _serviceBusMessageProcessor = serviceBusMessageProcessor;
            _logger = logger;
            _jobprofileMessageConverter = jobprofileMessageConverter;
            _session = session;
            _jobProfileIndexRepository = jobProfileIndexRepository;
        }

        public async Task ProcessContentContext(ContentContextBase context, string actionType)
        {
            try
            {
                switch (context.ContentItem.ContentType)
                {
                    case ContentTypes.JobProfile:
                        await GenerateServiceBusMessageForJobProfile(context, actionType);
                        break;

                    case ContentTypes.Environment:
                    case ContentTypes.Uniform:
                    case ContentTypes.Location:
                        await GenerateServiceBusMessageForWYDTypes(context, actionType);
                        break;

                    case ContentTypes.Universitylink:
                    case ContentTypes.Collegelink:
                    case ContentTypes.Apprenticeshiplink:
                        await GenerateServiceBusMessageForTextFieldTypes(context, actionType);
                        break;

                    case ContentTypes.Restriction:
                    case ContentTypes.Registration:
                    case ContentTypes.Apprenticeshiprequirements:
                    case ContentTypes.Collegerequirements:
                    case ContentTypes.Universityrequirements:
                        await GenerateServiceBusMessageForInfoTypes(context, actionType);
                        break;

                    case ContentTypes.Workinghoursdetail:
                    case ContentTypes.Workingpatterns:
                    case ContentTypes.HiddenAlternativeTitle:
                    case ContentTypes.Workingpatterndetail:
                    case ContentTypes.Universityentryrequirements:
                    case ContentTypes.Collegeentryrequirements:
                    case ContentTypes.JobProfileSpecialism:
                    case ContentTypes.Apprenticeshipentryrequirements:
                        await GenerateServiceBusMessageForOtherReferenceTypes(context, actionType);
                        break;

                    case ContentTypes.Skill:
                        await GenerateServiceBusMessageForSkillTypes(context, actionType);
                        break;

                    case ContentTypes.JobProfileSOC:
                        await GenerateServiceBusMessageForSocCodeType(context, actionType);
                        break;

                    case ContentTypes.SOCskillsmatrix:
                        await GenerateServiceBusMessageForSocSkillsMatrixType(context, actionType);
                        break;

                    //case ContentTypes.JobProfileSpecialism:
                    // apprenticeship-standards	
                    // apprenticeship-frameworks
                    // HiddenAlternativeTitle
                    // UniversityEntryRequirements
                    // CollegeEntryRequirements
                    // WorkingHoursDetails
                    // WorkingHoursDetails
                    // WorkingPatternDetails
                    // 
                    //break;

                    default:
                        _logger.LogInformation($"Content type '{context.ContentItem.ContentType}' for this item with ContentItemId = '{context.ContentItem.ContentItemId}'  is not proccessed for ServiceBus messaging");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to export data for item with ContentItemId = {context.ContentItem.ContentItemId}");
                throw;
            }
        }

        private async Task GenerateServiceBusMessageForJobProfile(ContentContextBase context, string actionType)
        {
            bool isSaved = actionType.Equals(ActionTypes.Published) || actionType.Equals(ActionTypes.Draft);
            var jobprofileMessage = isSaved
                ? await _jobprofileMessageConverter.ConvertFromAsync(context.ContentItem)
                : new JobProfileMessage()
                {
                    JobProfileId = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                    Title = context.ContentItem.Content.Title
                };

            await _serviceBusMessageProcessor.SendJobProfileMessage(jobprofileMessage, context.ContentItem.ContentType, actionType);
        }

        private async Task GenerateServiceBusMessageForWYDTypes(ContentContextBase context, string actionType)
        {
            bool isSaved = actionType.Equals(ActionTypes.Published) || actionType.Equals(ActionTypes.Draft);
            IList<WhatYouWillDoContentItem> jobprofileData = new List<WhatYouWillDoContentItem>();
            string fieldDescription = context.ContentItem.ContentType switch
            {
                ContentTypes.Environment => context.ContentItem.Content.Environment.Description.Html,
                ContentTypes.Uniform => context.ContentItem.Content.Uniform.Description.Html,
                ContentTypes.Location => context.ContentItem.Content.Location.Description.Html,
                _ => throw new ArgumentException("No valid match found"),
            };

            var matches = await _jobProfileIndexRepository.GetAll(b => b.Uniform != null && b.Uniform.Contains(context.ContentItem.ContentItemId) ||
                b.Environment != null && b.Environment.Contains(context.ContentItem.ContentItemId) ||
                b.Location != null && b.Location.Contains(context.ContentItem.ContentItemId)).ListAsync();

            foreach (var item in matches)
            {
                if (isSaved)
                {
                    jobprofileData.Add(new WhatYouWillDoContentItem()
                    {
                        Id = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                        Title = context.ContentItem.Content.TitlePart.Title,
                        Description = fieldDescription,
                        JobProfileId = Guid.Parse(item.GraphSyncPartId ?? string.Empty),
                        JobProfileTitle = item.JobProfileTitle,
                        Url = string.Empty, // TODO: Needs revisiting during integration to see if leaving it blank does not break anything in CUI"
                        IsNegative = false
                    });
                }
                else
                {
                    jobprofileData.Add(new WhatYouWillDoContentItem()
                    {
                        Id = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                        JobProfileId = Guid.Parse(item.GraphSyncPartId ?? string.Empty)
                    });

                }
            }


            await _serviceBusMessageProcessor.SendOtherRelatedTypeMessages(jobprofileData, context.ContentItem.ContentType, actionType);
        }

        private async Task GenerateServiceBusMessageForTextFieldTypes(ContentContextBase context, string actionType)
        {
            bool isSaved = actionType.Equals(ActionTypes.Published) || actionType.Equals(ActionTypes.Draft);
            IList<TextFieldContentItem> jobprofileData = new List<TextFieldContentItem>();
            string fieldText = context.ContentItem.ContentType switch
            {
                ContentTypes.Universitylink => context.ContentItem.Content.Universitylink.Text.Text,
                ContentTypes.Collegelink => context.ContentItem.Content.Collegelink.Text.Text,
                ContentTypes.Apprenticeshiplink => context.ContentItem.Content.Apprenticeshiplink.Text.Text,
                _ => throw new ArgumentException("No valid match found"),
            };

            string fieldUrl = context.ContentItem.ContentType switch
            {
                ContentTypes.Universitylink => context.ContentItem.Content.Universitylink.URL.Text,
                ContentTypes.Collegelink => context.ContentItem.Content.Collegelink.URL.Text,
                ContentTypes.Apprenticeshiplink => context.ContentItem.Content.Apprenticeshiplink.URL.Text,
                _ => throw new ArgumentException("No valid match found"),
            };

            var matches = await _jobProfileIndexRepository.GetAll(b => b.UniversityLinks != null && b.UniversityLinks.Contains(context.ContentItem.ContentItemId) ||
                b.CollegeLink != null && b.CollegeLink.Contains(context.ContentItem.ContentItemId) ||
                b.ApprenticeshipLink != null && b.ApprenticeshipLink.Contains(context.ContentItem.ContentItemId)).ListAsync();

            foreach (var item in matches)
            {
                if (isSaved)
                {
                    jobprofileData.Add(new TextFieldContentItem()
                    {
                        Id = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                        Title = context.ContentItem.DisplayText,
                        Text = fieldText,
                        JobProfileId = Guid.Parse(item.GraphSyncPartId ?? string.Empty),
                        JobProfileTitle = item.JobProfileTitle,
                        Url = fieldUrl, 
                    });
                }
                else
                {
                    jobprofileData.Add(new TextFieldContentItem()
                    {
                        Id = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                        JobProfileId = Guid.Parse(item.GraphSyncPartId ?? string.Empty)
                    });

                }
            }

            await _serviceBusMessageProcessor.SendOtherRelatedTypeMessages(jobprofileData, context.ContentItem.ContentType, actionType);
        }

        private async Task GenerateServiceBusMessageForInfoTypes(ContentContextBase context, string actionType)
        {

            bool isSaved = actionType.Equals(ActionTypes.Published) || actionType.Equals(ActionTypes.Draft);
            IList<InfoContentItem> jobprofileData = new List<InfoContentItem>();
            string fieldInfo = context.ContentItem.ContentType switch
            {
                ContentTypes.Restriction => context.ContentItem.Content.Restriction.Info.Html,
                ContentTypes.Registration => context.ContentItem.Content.Registration.Description.Html,
                ContentTypes.Apprenticeshiprequirements => context.ContentItem.Content.Apprenticeshiprequirements.Info.Html,
                ContentTypes.Collegerequirements => context.ContentItem.Content.Collegerequirements.Info.Html,
                ContentTypes.Universityrequirements => context.ContentItem.Content.Universityrequirements.Info.Html,
                _ => throw new ArgumentException("No valid match found"),
            };


            var matches = await _jobProfileIndexRepository.GetAll(b => b.Restriction != null && b.Restriction.Contains(context.ContentItem.ContentItemId) ||
                b.Registration != null && b.Registration.Contains(context.ContentItem.ContentItemId) ||
                b.ApprenticeshipRequirements != null && b.ApprenticeshipRequirements.Contains(context.ContentItem.ContentItemId) ||
                b.CollegeRequirements != null && b.CollegeRequirements.Contains(context.ContentItem.ContentItemId) ||
                b.UniversityRequirements != null && b.UniversityRequirements.Contains(context.ContentItem.ContentItemId)).ListAsync();

            foreach (var item in matches)
            {
                if (isSaved)
                {
                    jobprofileData.Add(new InfoContentItem()
                    {
                        Id = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                        Title = context.ContentItem.DisplayText,
                        Info = fieldInfo,
                        JobProfileId = Guid.Parse(item.GraphSyncPartId ?? string.Empty),
                        JobProfileTitle = item.JobProfileTitle,
                    });
                }
                else
                {
                    jobprofileData.Add(new InfoContentItem()
                    {
                        Id = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                        JobProfileId = Guid.Parse(item.GraphSyncPartId ?? string.Empty)
                    });

                }
            }

            await _serviceBusMessageProcessor.SendOtherRelatedTypeMessages(jobprofileData, context.ContentItem.ContentType, actionType);

        }

        private async Task GenerateServiceBusMessageForOtherReferenceTypes(ContentContextBase context, string actionType)
        {
            bool isSaved = actionType.Equals(ActionTypes.Published) || actionType.Equals(ActionTypes.Draft);
            IList<OtherReferenceFieldContentItem> jobprofileData = new List<OtherReferenceFieldContentItem>();
            string fieldDescription = context.ContentItem.ContentType switch
            {
                ContentTypes.Workinghoursdetail => context.ContentItem.Content.Workinghoursdetail.Description.Text,
                ContentTypes.Workingpatterns => context.ContentItem.Content.Workingpatterns.Description.Text,
                ContentTypes.HiddenAlternativeTitle => context.ContentItem.Content.HiddenAlternativeTitle.Description.Text,
                ContentTypes.Workingpatterndetail => context.ContentItem.Content.Workingpatterndetail.Description.Text,
                ContentTypes.Universityentryrequirements => context.ContentItem.Content.Universityentryrequirements.Description.Text,
                ContentTypes.Collegeentryrequirements => context.ContentItem.Content.Collegeentryrequirements.Description.Text,
                ContentTypes.JobProfileSpecialism => context.ContentItem.Content.JobProfileSpecialism.Description.Text,
                ContentTypes.Apprenticeshipentryrequirements => context.ContentItem.Content.Apprenticeshipentryrequirements.Description.Text,
                _ => throw new ArgumentException("No valid match found"),
            };

            var matches = await _jobProfileIndexRepository.GetAll(b => b.WorkingHoursDetail != null && b.WorkingHoursDetail.Contains(context.ContentItem.ContentItemId) ||
                b.WorkingPatterns != null && b.WorkingPatterns.Contains(context.ContentItem.ContentItemId) ||
                b.HiddenAlternativeTitle != null && b.HiddenAlternativeTitle.Contains(context.ContentItem.ContentItemId) ||
                b.WorkingPatternDetail != null && b.WorkingPatternDetail.Contains(context.ContentItem.ContentItemId) ||
                b.UniversityEntryRequirements != null && b.UniversityEntryRequirements.Contains(context.ContentItem.ContentItemId) ||
                b.CollegeEntryRequirements != null && b.CollegeEntryRequirements.Contains(context.ContentItem.ContentItemId) ||
                b.JobProfileSpecialism != null && b.JobProfileSpecialism.Contains(context.ContentItem.ContentItemId) ||
                b.ApprenticeshipEntryRequirements != null && b.ApprenticeshipEntryRequirements.Contains(context.ContentItem.ContentItemId)).ListAsync();

            foreach (var item in matches)
            {
                if (isSaved)
                {
                    jobprofileData.Add(new OtherReferenceFieldContentItem()
                    {
                        Id = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                        Title = context.ContentItem.Content.TitlePart.Title,
                        Description = fieldDescription,
                        JobProfileId = Guid.Parse(item.GraphSyncPartId ?? string.Empty),
                        JobProfileTitle = item.JobProfileTitle,
                        Url = string.Empty, // TODO: Needs revisiting during integration to see if leaving it blank does not break anything in CUI"
                    });
                }
                else
                {
                    jobprofileData.Add(new OtherReferenceFieldContentItem()
                    {
                        Id = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                        JobProfileId = Guid.Parse(item.GraphSyncPartId ?? string.Empty)
                    });

                }
            }


            await _serviceBusMessageProcessor.SendOtherRelatedTypeMessages(jobprofileData, context.ContentItem.ContentType, actionType);
        }

        private Task GenerateServiceBusMessageForSkillTypes(ContentContextBase context, string actionType)
        {
            var contentType = context.ContentItem.ContentType;

            // TODO: find all parents for the referenced content item.
            IEnumerable<SkillContentItem> jobprofileData = new List<SkillContentItem>()
                {
                    new SkillContentItem()
                    {
                        Id = new Guid(context.ContentItem.ContentItemId),
                        Title = context.ContentItem.Content.Title}
                };
            return _serviceBusMessageProcessor.SendOtherRelatedTypeMessages(jobprofileData, contentType, actionType);
        }

        private Task GenerateServiceBusMessageForSocCodeType(ContentContextBase context, string actionType)
        {
            var contentType = context.ContentItem.ContentType;

            // TODO: find all parents for the referenced content item.
            IEnumerable<SocCodeContentItem> jobprofileData = new List<SocCodeContentItem>()
                {
                    new SocCodeContentItem()
                    {
                        Id = new Guid(context.ContentItem.ContentItemId),
                        Title = context.ContentItem.Content.Title}
                };
            return _serviceBusMessageProcessor.SendOtherRelatedTypeMessages(jobprofileData, contentType, actionType);
        }

        private Task GenerateServiceBusMessageForSocSkillsMatrixType(ContentContextBase context, string actionType)
        {
            var contentType = context.ContentItem.ContentType;

            // TODO: find all parents for the referenced content item.
            IEnumerable<SocSkillMatrixContentItem> jobprofileData = new List<SocSkillMatrixContentItem>()
                {
                    new SocSkillMatrixContentItem()
                    {
                        Id = new Guid(context.ContentItem.ContentItemId),
                        Title = context.ContentItem.Content.Title}
                };
            return _serviceBusMessageProcessor.SendOtherRelatedTypeMessages(jobprofileData, contentType, actionType);
        }
    }
}
