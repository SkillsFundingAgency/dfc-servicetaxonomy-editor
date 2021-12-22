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


namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling
{
    internal class DataEventProcessor : IDataEventProcessor
    {
        private readonly IServiceBusMessageProcessor _serviceBusMessageProcessor;
        private readonly ILogger<DataEventProcessor> _logger;
        private readonly IMessageConverter<JobProfileMessage> _jobprofileMessageConverter;

        public DataEventProcessor(IServiceBusMessageProcessor serviceBusMessageProcessor,
                                    ILogger<DataEventProcessor> logger,
                                    IMessageConverter<JobProfileMessage> jobprofileMessageConverter)
        {
            _serviceBusMessageProcessor = serviceBusMessageProcessor;
            _logger = logger;
            _jobprofileMessageConverter = jobprofileMessageConverter;
        }

        public async Task ProcessContentContext(ContentContextBase context, string actionType)
        {
            try
            {
                switch (context.ContentItem.ContentType)
                {
                    case ContentTypes.JobProfile:
                        if (actionType.Equals(ActionTypes.Published) || actionType.Equals(ActionTypes.Draft))
                        {
                            await GenerateServiceBusMessageForJobProfile(context, actionType);
                        }
                        else
                        {
                            await GenerateServiceBusMessageForJobProfileUnPublish(context, actionType);
                        }

                        break;

                    //case Constants.Restriction:
                    case ContentTypes.Registration:
                        //case Constants.ApprenticeshipRequirement:
                        //case Constants.CollegeRequirement:
                        //case Constants.UniversityRequirement:
                        //    GenerateServiceBusMessageForInfoTypes(item, eventAction);
                        await GenerateServiceBusMessageForInfoTypes(context, actionType);
                        break;

                    //case Constants.Uniform:
                    //case Constants.Location:
                    //case Constants.Environment:
                    //    GenerateServiceBusMessageForWYDTypes(item, eventAction);

                    //    break;

                    //case Constants.UniversityLink:
                    //case Constants.CollegeLink:
                    //case Constants.ApprenticeshipLink:
                    //    GenerateServiceBusMessageForTextFieldTypes(item, eventAction);

                    //    break;

                    //case Constants.Skill:
                    //    GenerateServiceBusMessageForSkillTypes(item, eventAction);

                    //    break;

                    //case Constants.JobProfileSoc:
                    //    GenerateServiceBusMessageForSocCodeType(item, eventAction);

                    //    break;

                    //case Constants.SOCSkillsMatrix:

                    //    //For all the Dynamic content types we are using Jobprofile as Parent Type
                    //    //and for only Skills we are using SocSkillsMatrix Type as the Parent Type
                    //    var liveVersionItem = item;

                    //    if (item.Status.ToString() != Constants.ItemStatusLive)
                    //    {
                    //        var liveItem = dynamicModuleManager.Lifecycle.GetLive(item);
                    //        liveVersionItem = dynamicModuleManager.GetDataItem(item.GetType(), liveItem.Id);
                    //    }
                    //    else
                    //    {
                    //        var masterItem = dynamicModuleManager.Lifecycle.GetMaster(item);
                    //        item = dynamicModuleManager.GetDataItem(item.GetType(), masterItem.Id);
                    //    }

                    //    SkillsMatrixParentItems = GetParentItemsForSocSkillsMatrix(item);
                    //    GenerateServiceBusMessageForSocSkillsMatrixType(liveVersionItem, item, eventAction);

                    //    break;

                    default:
                        _logger.LogInformation($"Content type '{context.ContentItem.ContentType}' for this item with ContentItemId = '{context.ContentItem.ContentItemId}'  is not proccessed for ServiceBus messaging");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to export data for item with ContentItemId = {context.ContentItem.ContentItemId}", ex);
                throw;
            }
        }

        private async Task GenerateServiceBusMessageForJobProfile(ContentContextBase context, string actionType)
        {
            var contentType = context.ContentItem.ContentType;
            if (contentType != null && contentType == ContentTypes.JobProfile)
            { 
                var jobprofileMessage = _jobprofileMessageConverter.ConvertFrom(context.ContentItem);
                await _serviceBusMessageProcessor.SendJobProfileMessage(jobprofileMessage, contentType, actionType).ConfigureAwait(false);
            }
        }

        private async Task GenerateServiceBusMessageForJobProfileUnPublish(ContentContextBase context, string actionType)
        {
            //var jobProfileMessage = new JobProfileMessage();
            //jobProfileMessage.JobProfileId = item.OriginalContentId;
            //jobProfileMessage.Title = dynamicContentExtensions.GetFieldValue<Lstring>(item, nameof(JobProfileMessage.Title));
            //serviceBusMessageProcessor.SendJobProfileMessage(jobProfileMessage, item.GetType().Name, eventAction.ToString());
            var contentType = context.ContentItem.ContentType;
            if (contentType != null && contentType == ContentTypes.JobProfile)
            {
                JobProfileMessage jobprofileData = new JobProfileMessage()
                {
                    JobProfileId = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                    Title = context.ContentItem.Content.Title
                };
                await _serviceBusMessageProcessor.SendJobProfileMessage(jobprofileData, contentType, actionType); 
            }
        }

        private async Task GenerateServiceBusMessageForInfoTypes(ContentContextBase context, string actionType)
        {
            //DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager(Constants.DynamicProvider);
            //var contentLinksManager = ContentLinksManager.GetManager();
            //var parentItemContentLinks = contentLinksManager.GetContentLinks()
            //       .Where(c => c.ParentItemType == ParentType && c.ChildItemId == item.Id)
            //       .Select(c => c.ParentItemId).ToList();
            //var relatedInfoTypes = GetInfoRelatedItems(item, parentItemContentLinks, dynamicModuleManager, ParentType);
            //serviceBusMessageProcessor.SendOtherRelatedTypeMessages(relatedInfoTypes, item.GetType().Name, eventAction.ToString());
            var contentType = context.ContentItem.ContentType;

            if (contentType != null && contentType == ContentTypes.Registration)
            {
                IEnumerable<InfoContentItem> jobprofileData = new List<InfoContentItem>()
                {
                    new InfoContentItem()
                    {
                        Id = new Guid(context.ContentItem.ContentItemId),
                        Title = context.ContentItem.Content.Title}
                };
                await _serviceBusMessageProcessor.SendOtherRelatedTypeMessages(jobprofileData, contentType, actionType);
            }
        }
    }
}
