using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters
{
    public class WhatYouWillDoMessageConverter : IMessageConverter<WhatYouWillDoData>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WhatYouWillDoMessageConverter> _logger;

        public WhatYouWillDoMessageConverter(IServiceProvider serviceProvider, ILogger<WhatYouWillDoMessageConverter> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<WhatYouWillDoData> ConvertFromAsync(ContentItem contentItem)
        {
            try
            {
                var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
                List<ContentItem> relatedLocations = GetContentItems(contentItem.Content.JobProfile.Relatedlocations, contentManager);
                List<ContentItem> relatedEnvironments = GetContentItems(contentItem.Content.JobProfile.Relatedenvironments, contentManager);
                List<ContentItem> relatedUniforms = GetContentItems(contentItem.Content.JobProfile.Relateduniforms, contentManager);

                var whatYouWillDoData = new WhatYouWillDoData
                {
                    DailyTasks = contentItem.Content.JobProfile.Daytodaytasks.Html,
                    Locations = GetWYDRelatedItems(relatedLocations),
                    Environments = GetWYDRelatedItems(relatedEnvironments),
                    Uniforms = GetWYDRelatedItems(relatedUniforms)
                };
                return whatYouWillDoData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        private List<ContentItem> GetContentItems(dynamic contentPicker, IContentManager contentManager)
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

        private IEnumerable<WYDRelatedContentType> GetWYDRelatedItems(List<ContentItem> contentItems)
        {
            var relatedContentTypes = new List<WYDRelatedContentType>();
            if (contentItems.Any())
            {
                foreach (var contentItem in contentItems)
                {
                    relatedContentTypes.Add(new WYDRelatedContentType
                    {
                        Url = contentItem.Content.GraphSyncPart.Text,
                        Description = GetWYDRelatedItemDescription(contentItem),
                        Title = contentItem.As<TitlePart>().Title,
                    });
                }
            }

            return Enumerable.Empty<WYDRelatedContentType>();
        }

        private static string GetWYDRelatedItemDescription(ContentItem contentItem) =>
            contentItem.ContentType switch
            {
                ContentTypes.Location => contentItem.Content.Location.Description.Html,
                ContentTypes.Environment => contentItem.Content.Environment.Description.Html,
                ContentTypes.Uniform => contentItem.Content.Uniform.Description.Html,
                _ => string.Empty,
            };
    }
}


