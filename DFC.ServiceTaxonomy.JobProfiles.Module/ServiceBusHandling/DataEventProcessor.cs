using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;

using Microsoft.Extensions.Logging;

using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

using YesSql;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling
{
    internal class DataEventProcessor : IDataEventProcessor
    {
        private readonly IServiceBusMessageProcessor _serviceBusMessageProcessor;
        private readonly ISession _session;
        private readonly ILogger<DataEventProcessor> _logger;
        private readonly IMessageConverter<JobProfileMessage> _jobprofileMessageConverter;

        public DataEventProcessor(IServiceBusMessageProcessor serviceBusMessageProcessor,
                                    ILogger<DataEventProcessor> logger,
                                    IMessageConverter<JobProfileMessage> jobprofileMessageConverter,
                                    ISession session)
        {
            _serviceBusMessageProcessor = serviceBusMessageProcessor;
            _logger = logger;
            _jobprofileMessageConverter = jobprofileMessageConverter;
            _session = session;
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

        private Task GenerateServiceBusMessageForWYDTypes(ContentContextBase context, string actionType)
        {
            // TODO: find all parents for the referenced content item.
            IEnumerable<WhatYouWillDoContentItem> jobprofileData = new List<WhatYouWillDoContentItem>
                {
                    new WhatYouWillDoContentItem()
                    {
                        Id = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                        Title = context.ContentItem.Content.Title}
                };
            return _serviceBusMessageProcessor.SendOtherRelatedTypeMessages(jobprofileData, context.ContentItem.ContentType, actionType);
        }

        private Task GenerateServiceBusMessageForTextFieldTypes(ContentContextBase context, string actionType)
        {
            var contentType = context.ContentItem.ContentType;

            // TODO: find all parents for the referenced content item.
            IEnumerable<TextFieldContentItem> jobprofileData = new List<TextFieldContentItem>
                {
                    new TextFieldContentItem()
                    {
                        Id = new Guid(context.ContentItem.ContentItemId),
                        Title = context.ContentItem.Content.Title}
                };
            return _serviceBusMessageProcessor.SendOtherRelatedTypeMessages(jobprofileData, contentType, actionType);
        }

        private Task GenerateServiceBusMessageForInfoTypes(ContentContextBase context, string actionType)
        {
            var contentType = context.ContentItem.ContentType;

            // TODO: find all parents for the referenced content item.
            IEnumerable<InfoContentItem> jobprofileData = new List<InfoContentItem>()
                {
                    new InfoContentItem()
                    {
                        Id = new Guid(context.ContentItem.ContentItemId),
                        Title = context.ContentItem.Content.Title}
                };
            return _serviceBusMessageProcessor.SendOtherRelatedTypeMessages(jobprofileData, contentType, actionType);
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
