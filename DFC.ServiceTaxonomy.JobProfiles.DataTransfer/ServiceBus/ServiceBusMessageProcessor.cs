using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus.Configuration;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Interfaces;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus
{
    internal class ServiceBusMessageProcessor : IServiceBusMessageProcessor
    {
        private readonly ServiceBusSettings _serviceBusSettings;
        private readonly ILogger<ServiceBusMessageProcessor> _logger;

        public ServiceBusMessageProcessor(IConfiguration configuration, ILogger<ServiceBusMessageProcessor> logger)
        {
            _serviceBusSettings = configuration.GetSection("ServiceBusSettings").Get<ServiceBusSettings>();
            _logger = logger;
        }

        public async Task SendJobProfileMessage(JobProfileMessage jpData, string contentType, string actionType)
        {
            _logger.LogInformation($" CREATED service bus message for OrchardCore event {actionType.ToUpper()} on JobProfile with Title -- {jpData.Title} and Jobprofile Id -- {jpData.JobProfileId}");

            var topicName = (actionType == "Draft")
                ? _serviceBusSettings.ServiceBusTopicNameForDraft
                : _serviceBusSettings.ServiceBusTopicName;

            var topicClient = new TopicClient(_serviceBusSettings.ServiceBusConnectionString, topicName);

            // Send Messages
            var jsonData = JsonConvert.SerializeObject(jpData);
            try
            {
                _logger.LogInformation($" SENDING service bus message for OrchardCore event {actionType.ToUpper()} on JobProfile with Title -- {jpData.Title} and with Jobprofile Id -- {jpData.JobProfileId} ");

                // Message that send to the queue
                var message = new Message(Encoding.UTF8.GetBytes(jsonData));
                message.ContentType = "application/json";
                message.Label = jpData.Title;
                message.UserProperties.Add("Id", jpData.JobProfileId);
                message.UserProperties.Add("ActionType", actionType);
                message.UserProperties.Add("CType", contentType);
                message.CorrelationId = Guid.NewGuid().ToString();

                await topicClient.SendAsync(message);

                _logger.LogInformation($" SENT service bus message for OrchardCore event {actionType.ToUpper()} on JobProfile with Title -- {jpData.Title} with Jobprofile Id -- {jpData.JobProfileId} and with Correlation Id -- {message.CorrelationId.ToString()}");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($" FAILED service bus message for OrchardCore event {actionType.ToUpper()} on JobProfile with Title -- {jpData.Title} and with Jobprofile Id -- {jpData.JobProfileId} has an exception \n {ex.Message} ");
            }
            finally
            {
                await topicClient.CloseAsync();
            }
        }

        public async Task SendOtherRelatedTypeMessages(IEnumerable<RelatedContentItem> relatedContentItems, string contentType, string actionType)
        {
            _logger.LogInformation($" CREATED service bus message for OrchardCore event {actionType.ToUpper()} on Item of Type -- {contentType.ToUpper()} with {relatedContentItems.Count().ToString()} message(s)");

            var topicClient = new TopicClient(_serviceBusSettings.ServiceBusConnectionString, _serviceBusSettings.ServiceBusTopicName);
            try
            {
                foreach (var relatedContentItem in relatedContentItems)
                {
                    _logger.LogInformation($" SENDING service bus message for OrchardCore event {actionType.ToUpper()}  on Item -- {relatedContentItem.Title} of Type -- {contentType} with Id -- {relatedContentItem.Id.ToString()} linked to Job Profile {relatedContentItem.JobProfileTitle} -- {relatedContentItem.JobProfileId.ToString()}");

                    // Send Messages
                    var jsonData = JsonConvert.SerializeObject(relatedContentItem);

                    // Message that send to the queue
                    var message = new Message(Encoding.UTF8.GetBytes(jsonData));
                    message.ContentType = "application/json";
                    message.Label = relatedContentItem.Title;
                    message.UserProperties.Add("Id", $"{relatedContentItem.JobProfileId}--{relatedContentItem.Id}");
                    message.UserProperties.Add("ActionType", actionType);
                    message.UserProperties.Add("CType", contentType);
                    message.CorrelationId = Guid.NewGuid().ToString();

                    await topicClient.SendAsync(message);

                    _logger.LogInformation($" SENT service bus message for OrchardCore event {actionType.ToUpper()} on Item -- {relatedContentItem.Title} of Type -- {contentType} with Id -- {relatedContentItem.Id.ToString()} linked to Job Profile {relatedContentItem.JobProfileTitle} -- {relatedContentItem.JobProfileId.ToString()} with Correlation Id -- {message.CorrelationId.ToString()}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($" FAILED service bus message for OrchardCore event {actionType.ToUpper()} on Item of Type -- {contentType} with {relatedContentItems.Count().ToString()} message(s) has an exception \n {ex.Message}");
            }
            finally
            {
                await topicClient.CloseAsync();
            }
        }
    }
}
